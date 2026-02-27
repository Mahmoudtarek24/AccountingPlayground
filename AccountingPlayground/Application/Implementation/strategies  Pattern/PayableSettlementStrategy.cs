using AccountingPlayground.Application.Dto_s;
using AccountingPlayground.Application.Enums;
using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Domain.Interfaces;
using AccountingPlayground.Infrastructure.Context;
using AccountingPlayground.Infrastructure.Implementation;
using Microsoft.EntityFrameworkCore;

namespace AccountingPlayground.Application.Implementation.strategies__Pattern
{
    public class PayableSettlementStrategy : BasePaymentVoucherStrategy
    {
        private readonly JournalEntryRepository journalEntryRepository; 
        public PayableSettlementStrategy(ApplicationDbContext context,
                                         JournalEntryService journalEntryService,
                                         JournalEntryRepository journalEntryRepository)
            : base(context, journalEntryService) 
        {
            this.journalEntryRepository = journalEntryRepository;   
        }
        public override SettlementType Type => SettlementType.PayableSettlement;    

        public override async Task<List<string>> Validate(CreatePaymentVoucherDto dto)
        {
            var errors = new List<string>();

            if (dto.SupplierId is null)
                errors.Add("SupplierId is required for Payable Settlement.");   
            else
            {
                var supplierExists = context.Suppliers.Any(s => s.Id == dto.SupplierId.Value);
                if (!supplierExists)
                    errors.Add($"Supplier with Id {dto.SupplierId.Value} does not exist.");
            }

            if (dto.Lines.Any(l => l.PurchaseInvoiceId is null))
                errors.Add("Each line must reference a purchase invoice");

            if (dto.Lines.Any(l => !l.FinancialAccountId.HasValue))
                errors.Add("Each line must have a FinancialAccountId");

            // 5. مفيش VAT في Payable Settlement (اتسجلت في الفاتورة)
            if (dto.Lines.Any(l => l.VatAccountId.HasValue))
                errors.Add("Payable Settlement should not have VAT (already recorded in invoice)");


            // Validate Payment Account
            await ValidateAccountForRole(dto.PaymentAccountId, AccountRole.CashBank, errors);

            if (dto.Lines.Any(l => l.PurchaseInvoiceId is null))
                errors.Add("Payable Settlement should reference a registered invoice");


             // Liabilities(خصوم) ← Parent
                  //└── Current Liabilities(خصوم متداولة)
                  //     └── Accounts Payable(2100) (الدائنون) ← هنا
                  //         ├── مورد اللحوم(2101)    ← Leaf
                  //         ├── مورد الخضار(2102)    ← Leaf
                  //         └── شركة الكهرباء(2103)  ← Leaf

            // Validate Lines
            foreach (var line in dto.Lines)
                if (line.FinancialAccountId.HasValue)
                    await ValidateAccountForRole(line.FinancialAccountId.Value, AccountRole.Payable, errors);

            var PayableAccountId = dto.Lines[0].FinancialAccountId.Value;
            var supplierBalance = await context.JournalEntryLines
                    .Where(e => e.FinancialAccountId == PayableAccountId)
                    .SumAsync(e => e.Credit - e.Debit);

            // لو المبلغ اللي هتدفعه أكبر من رصيد المورد كله
            if (dto.Lines.Sum(l => l.Amount) > supplierBalance)
                errors.Add("Payment amount exceeds total supplier balance");

            // هنجيب رصيد كل فاتوره علشان نعرف علشان نعرف هل الفاتوره دي هتتسدد بالكامل ولا جزئي    

            foreach(var line in dto.Lines)
            {
                var invoice = await context.PurchaseInvoices
                    .Where(i => i.Id == line.PurchaseInvoiceId.Value)
                    .Select(i => new { i.Id, i.SupplierId, i.TotalAmount })
                    .FirstOrDefaultAsync();

                var totalPaid = await context.PaymentVoucherLines
                                    .Where(e => e.PurchaseInvoiceId == line.PurchaseInvoiceId
                                            && e.PaymentVoucher.Status == VoucherStatus.Posted
                                            && !e.PaymentVoucher.IsReversed)
                                    .SumAsync(pvl => pvl.Amount);

                var remainingBalance = invoice.TotalAmount - totalPaid;

                if (line.Amount > remainingBalance)
                    errors.Add($"Invoice {invoice.Id}: amount {line.Amount} exceeds remaining balance {remainingBalance}");
            }

            return errors;  
        }
        protected override async Task BuildVoucherLines(CreatePaymentVoucherDto dto, PaymentVoucher voucher)
        {
            foreach (var line in dto.Lines)
                voucher.Lines.Add(new PaymentVoucherLine
                {
                    FinancialAccountId = line.FinancialAccountId!.Value,
                    Amount = line.Amount,
                    VatAmount = null,
                    TotalAmount = line.Amount,
                    PurchaseInvoiceId = line.PurchaseInvoiceId!.Value,
                });
        }

        protected override JournalEntryPostModel BuildJournalEntry(CreatePaymentVoucherDto dto, PaymentVoucher voucher)
        {
            var journalEntry = new JournalEntryPostModel
            {
                Reference = $"PV-{voucher.VoucherNo}",
                EntryDate = dto.VoucherDate,
                Lines = new List<JournalEntryLinePostModel>()
            };

            // Credit: الخزنة / البنك (الفلوس اللي طلعت)
            journalEntry.Lines.Add(new JournalEntryLinePostModel
            {
                FinancialAccountId = dto.PaymentAccountId,
                Debit = 0,
                Credit = (long)dto.Lines.Sum(l => l.Amount)
            });

            foreach (var line in dto.Lines)
            {
                journalEntry.Lines.Add(new JournalEntryLinePostModel
                {
                    FinancialAccountId = line.FinancialAccountId!.Value,
                    Debit = (long)line.Amount,
                    Credit = 0
                });
            }

            return journalEntry;
        }
    }
}

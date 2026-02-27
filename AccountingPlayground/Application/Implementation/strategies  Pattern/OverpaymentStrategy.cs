using AccountingPlayground.Application.Dto_s;
using AccountingPlayground.Application.Enums;
using AccountingPlayground.Application.Implementation;
using AccountingPlayground.Application.Implementation.strategies__Pattern;
using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

public class OverpaymentStrategy : BasePaymentVoucherStrategy
{
    public OverpaymentStrategy(ApplicationDbContext context, JournalEntryService journalEntryService)
        : base(context, journalEntryService) { }

    public override SettlementType Type => SettlementType.Overpayment;

    public override async Task<List<string>> Validate(CreatePaymentVoucherDto dto)
    {
        var errors = new List<string>();

        if (dto.Lines == null || !dto.Lines.Any())
        {
            errors.Add("At least one line is required");
            return errors;
        }

        // 1. لازم يكون فيه مورد
        if (dto.SupplierId is null)
            errors.Add("SupplierId is required for Overpayment");

        // 2. لازم يكون فيه على الأقل 2 Lines
        // واحد للفاتورة وواحد للزيادة
        if (dto.Lines.Count < 2)
            errors.Add("Overpayment requires at least 2 lines: invoice payment and overpayment amount");

        // 3. Amount لازم يكون موجب
        if (dto.Lines.Any(l => l.Amount <= 0))
            errors.Add("Amount must be positive");

        // 4. مفيش VAT
        if (dto.Lines.Any(l => l.VatAccountId.HasValue))
            errors.Add("Overpayment should not have VAT");

        // 5. لازم يكون فيه Line واحد على الأقل مربوط بفاتورة
        var invoiceLines = dto.Lines.Where(l => l.PurchaseInvoiceId.HasValue).ToList();
        if (!invoiceLines.Any())
            errors.Add("Overpayment must have at least one invoice line");

        // 6. لازم يكون فيه Line واحد على الأقل للزيادة (بدون فاتورة)
        var overpaymentLines = dto.Lines.Where(l => !l.PurchaseInvoiceId.HasValue).ToList();
        if (!overpaymentLines.Any())
            errors.Add("Overpayment must have at least one overpayment line");

        // 7. Validate Payment Account
        await ValidateAccountForRole(dto.PaymentAccountId, AccountRole.CashBank, errors);

        // 8. Validate كل Line
        foreach (var line in dto.Lines)
        {
            if (!line.FinancialAccountId.HasValue)
            {
                errors.Add("FinancialAccountId is required");
                continue;
            }

            if (line.PurchaseInvoiceId.HasValue)
            {
                // Line فاتورة → لازم يكون Liability (Accounts Payable)
                await ValidateAccountForRole(line.FinancialAccountId.Value, AccountRole.Payable, errors);

                // Validate الفاتورة موجودة وبتاعت نفس المورد
                var invoice = await context.PurchaseInvoices
                    .Where(i => i.Id == line.PurchaseInvoiceId.Value)
                    .Select(i => new { i.Id, i.SupplierId, i.TotalAmount })
                    .FirstOrDefaultAsync();

                if (invoice == null)
                {
                    errors.Add($"Invoice {line.PurchaseInvoiceId.Value} not found");
                    continue;
                }

                if (invoice.SupplierId != dto.SupplierId)
                    errors.Add($"Invoice {invoice.Id} does not belong to supplier {dto.SupplierId}");

                // Validate الرصيد المتبقي
                var totalPaid = await context.PaymentVoucherLines
                    .Where(pvl => pvl.PurchaseInvoiceId == invoice.Id
                        && pvl.PaymentVoucher.Status == VoucherStatus.Posted
                        && !pvl.PaymentVoucher.IsReversed)
                    .SumAsync(pvl => pvl.Amount);

                var remainingBalance = invoice.TotalAmount - totalPaid;

                if (line.Amount > remainingBalance)
                    errors.Add($"Invoice {invoice.Id}: amount {line.Amount} exceeds remaining balance {remainingBalance}");
            }
            else
            {
                // Line زيادة → لازم يكون Asset (Supplier Debit Balance)
                await ValidateAccountForRole(line.FinancialAccountId.Value, AccountRole.SupplierDebitBalance, errors);
            }
        }

        return errors;
    }

    protected override async Task BuildVoucherLines(CreatePaymentVoucherDto dto, PaymentVoucher voucher)
    {
        foreach (var line in dto.Lines)
        {
            voucher.Lines.Add(new PaymentVoucherLine
            {
                FinancialAccountId = line.FinancialAccountId!.Value,
                Amount = line.Amount,
                VatAmount = null,
                TotalAmount = line.Amount,
                PurchaseInvoiceId = line.PurchaseInvoiceId
            });
        }
    }

    protected override JournalEntryPostModel BuildJournalEntry(CreatePaymentVoucherDto dto, PaymentVoucher voucher)
    {
        var journalEntry = new JournalEntryPostModel
        {
            Reference = $"PV-{voucher.VoucherNo}",
            EntryDate = dto.VoucherDate,
            Lines = new List<JournalEntryLinePostModel>()
        };

        // Debit: كل الـ Lines (فاتورة + زيادة)
        foreach (var line in dto.Lines)
        {
            journalEntry.Lines.Add(new JournalEntryLinePostModel
            {
                FinancialAccountId = line.FinancialAccountId!.Value,
                Debit = (long)line.Amount,
                Credit = 0
            });
        }

        // Credit: الخزنة / البنك (المبلغ الكامل)
        journalEntry.Lines.Add(new JournalEntryLinePostModel
        {
            FinancialAccountId = dto.PaymentAccountId,
            Debit = 0,
            Credit = (long)dto.Lines.Sum(l => l.Amount)
        });

        return journalEntry;
    }
}
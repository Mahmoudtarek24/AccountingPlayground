using AccountingPlayground.Application.Dto_s;
using AccountingPlayground.Application.Enums;
using AccountingPlayground.Application.Results;
using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Domain.AccountingEntities.Enums;
using AccountingPlayground.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AccountingPlayground.Application.Implementation.strategies__Pattern
{
    public class DirectExpenseStrategy : BasePaymentVoucherStrategy
    {
        public DirectExpenseStrategy(ApplicationDbContext context, JournalEntryService journalEntryService)
            : base(context, journalEntryService) { }

        public override  SettlementType Type => SettlementType.DirectExpense;

        public override async Task<List<string>> Validate(CreatePaymentVoucherDto dto)
        {
            var errors = new List<string>();

            // 2. Direct Expense مفيهوش فاتورة مسجلة في النظام
            if (dto.Lines.Any(l => l.PurchaseInvoiceId != null))
                errors.Add("Direct expense should not reference a registered invoice");

            if (dto.Lines.Any(l => l.Amount <= 0))
                errors.Add("Amount must be positive");

            if (dto.Lines.Any(l => l.FinancialAccountId.HasValue == l.VatAccountId.HasValue))
                errors.Add("Each line must have either FinancialAccountId or VatAccountId, not both or neither");

            // Validate Payment Account
            await ValidateAccountForRole(dto.PaymentAccountId, AccountRole.CashBank, errors);

            // Validate Lines
            foreach (var line in dto.Lines)
            {
                if (line.FinancialAccountId.HasValue)
                    await ValidateAccountForRole(line.FinancialAccountId.Value, AccountRole.Expense, errors);

                if (line.VatAccountId.HasValue)
                    await ValidateAccountForRole(line.VatAccountId.Value, AccountRole.VatRecoverable, errors);
            }

            return errors;
        }
        // ✅ الـ Lines الخاصة بـ Direct Expense
        protected override async Task BuildVoucherLines(CreatePaymentVoucherDto dto, PaymentVoucher voucher)
        {
            foreach (var line in dto.Lines)
                voucher.Lines.Add(new PaymentVoucherLine
                {
                    FinancialAccountId = line.FinancialAccountId ?? line.VatAccountId!.Value,
                    Amount = line.FinancialAccountId.HasValue ? line.Amount : 0,
                    VatAmount = line.VatAccountId.HasValue ? line.Amount : 0,
                    TotalAmount = line.Amount,
                    PurchaseInvoiceId = line.PurchaseInvoiceId
                });
        }

        // ✅ القيد المحاسبي الخاص بـ Direct Expense
        protected override JournalEntryPostModel BuildJournalEntry(CreatePaymentVoucherDto dto,PaymentVoucher voucher)
        {
            var journalEntry = new JournalEntryPostModel
            {
                Reference = $"PV-{voucher.VoucherNo}",
                EntryDate = dto.VoucherDate,
            };

            // Credit: الخزنة / البنك
            journalEntry.Lines.Add(new JournalEntryLinePostModel
            {
                FinancialAccountId = dto.PaymentAccountId,
                Debit = 0,
                Credit = (long)dto.Lines.Sum(l => l.Amount)
            });

            // Debit: المصروفات والضريبة
            foreach (var line in dto.Lines)
            {
                journalEntry.Lines.Add(new JournalEntryLinePostModel
                {
                    FinancialAccountId = (line.FinancialAccountId ?? line.VatAccountId)!.Value,
                    Debit = (long)line.Amount,
                    Credit = 0
                });
            }

            return journalEntry;
        }
    }
}

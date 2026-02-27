using AccountingPlayground.Application.Dto_s;
using AccountingPlayground.Application.Enums;
using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AccountingPlayground.Application.Implementation.strategies__Pattern
{
    public class AdvancePaymentStrategy : BasePaymentVoucherStrategy
    {
        public AdvancePaymentStrategy(ApplicationDbContext context, JournalEntryService journalEntryService)
                 : base(context, journalEntryService) { }
        public override SettlementType Type => SettlementType.AdvancePayment;

        public override async Task<List<string>> Validate(CreatePaymentVoucherDto dto)
        {
            var errors = new List<string>();    
            var line = dto.Lines.FirstOrDefault();

            await ValidateAccountForRole(line.FinancialAccountId!.Value, AccountRole.AdvancePayment, errors);

            if (line.PurchaseInvoiceId is not null)
                errors.Add("handle message");

            return errors;
        }

        protected override async Task BuildVoucherLines(CreatePaymentVoucherDto dto, PaymentVoucher voucher)
        {
            var line = dto.Lines.First();
            voucher.Lines.Add(new PaymentVoucherLine
            {
                FinancialAccountId = line.FinancialAccountId!.Value,
                Amount = line.Amount,
                VatAmount = 0,
                TotalAmount = line.Amount,
                PurchaseInvoiceId = line.PurchaseInvoiceId
            }); 
        }
        protected override JournalEntryPostModel BuildJournalEntry(CreatePaymentVoucherDto dto, PaymentVoucher voucher)
        {
            var line = dto.Lines.FirstOrDefault();
            var journalEntry = new JournalEntryPostModel
            {
                Reference = $"PV-{voucher.VoucherNo}",
                EntryDate = dto.VoucherDate,
                Lines = new List<JournalEntryLinePostModel>()
            };

            journalEntry.Lines.Add(new JournalEntryLinePostModel
            {
                FinancialAccountId = dto.PaymentAccountId,
                Credit =  (long) line!.Amount,
                Debit = 0
            });     

            journalEntry.Lines.Add(new JournalEntryLinePostModel
            {
                FinancialAccountId = line.FinancialAccountId!.Value,
                Credit = 0,
                Debit = (long) line.Amount
            });     

            return journalEntry;
        }
    }
}

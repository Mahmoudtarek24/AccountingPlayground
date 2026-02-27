using AccountingPlayground.Application.Dto_s;
using AccountingPlayground.Application.Enums;
using AccountingPlayground.Application.Implementation.strategies__Pattern;
using AccountingPlayground.Application.Interfaces;
using AccountingPlayground.Application.Results;
using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Domain.Entities;
using AccountingPlayground.Domain.Interfaces;
using AccountingPlayground.Infrastructure.Context;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace AccountingPlayground.Application.Implementation
{
    public class PaymentVoucherService : IPaymentVoucherService
    {
        private List<IPaymentVoucherStrategy> _strategies;
        private readonly ApplicationDbContext _context;
        private readonly JournalEntryService _journalEntryService;

        public PaymentVoucherService(IEnumerable<IPaymentVoucherStrategy> strategies,
                                     ApplicationDbContext context,
                                     JournalEntryService journalEntryService)
        {
            _strategies = strategies.ToList();
            _context = context;
            _journalEntryService = journalEntryService;
        }

        public async Task<List<string>> CreatePaymentVoucher(CreatePaymentVoucherDto dto)
        {
            var strategy = _strategies.FirstOrDefault(e=>e.Type==dto.VoucherType);
            
            if (strategy == null)
                return new List<string> { "Invalid voucher type" };

            return await strategy.ExecuteAsync(dto);    
        }

        public async Task<bool> ReversePaymentVoucher(int voucherId, long? partialPayment = null)
        {
            var payment = await _context.PaymentVouchers.Include(e => e.JournalEntry)
                   .Include(e => e.Lines).SingleOrDefaultAsync(e => e.Id == voucherId);

            if (payment is null)
                return false;

            if (payment.Status != VoucherStatus.Posted)
                return false;

            if (payment.IsReversed)
                return false;

            // 4. TODO: لو اتعملت عليه تسوية بنكية ممنوع يتعكس
            // if (payment.IsReconciled) return false;


            // 5. Build Reversal Options
            ReversalOptions reversalOptions = null;
            if (partialPayment is not null)
                reversalOptions = new ReversalOptions
                {
                    PartialPayment = partialPayment,
                    ReversalDate = DateTime.UtcNow,
                };

            // 6. اعكس القيد المحاسبي
            var result = await _journalEntryService.ReverseJournalEntry(payment.JournalEntryId!.Value, reversalOptions);

            if (!result)
                return false;

            payment.IsReversed = true;
            payment.Status = VoucherStatus.Reversed;
            // 8. اعمل Voucher عكسي جديد
            var reversalVoucher = new PaymentVoucher
            {
                VoucherDate = DateTime.UtcNow,
             //   VoucherNo = await GeneratePaymentVoucherNo(),
                PaymentMethod = payment.PaymentMethod,
                Status = VoucherStatus.Posted,
                TotalAmount = partialPayment ?? payment.TotalAmount,
                SupplierId = payment.SupplierId,
                EmployeeId = payment.EmployeeId,
                CashSessionId = payment.CashSessionId,
                IsReversed = false,
                OriginalVoucherId = payment.Id, // ✅ مربوط بالأصلي
            };

            await _context.PaymentVouchers.AddAsync(reversalVoucher);
            await _context.SaveChangesAsync();

            return true;
        }


        //public double PercentageWithholding(Supplier supplier)
        //{
        //    return supplier switch
        //    {
        //        { TaxRegistrationStatus: TaxRegistrationStatus.Exempt } => 0.0,
        //        { TaxRegistrationStatus: TaxRegistrationStatus.Registered, IsSubjectToWithholding: true } => 0.01,
        //        { TaxRegistrationStatus: TaxRegistrationStatus.NotRegistered, IsSubjectToWithholding: true } => 0.05,
        //        _ => 0.0,
        //    };
        //}
    }
}

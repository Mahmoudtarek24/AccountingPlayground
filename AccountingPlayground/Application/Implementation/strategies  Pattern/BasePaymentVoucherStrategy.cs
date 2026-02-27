using AccountingPlayground.Application.Dto_s;
using AccountingPlayground.Application.Enums;
using AccountingPlayground.Application.Interfaces;
using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AccountingPlayground.Application.Implementation.strategies__Pattern
{
    public abstract class BasePaymentVoucherStrategy : IPaymentVoucherStrategy
    {
        protected readonly ApplicationDbContext context;
        protected readonly JournalEntryService journalEntryService;
        public abstract SettlementType Type { get; }
       
        // ✅ كل Strategy لازم تعمل implement للـ Validation بتاعتها
        public abstract Task<List<string>> Validate(CreatePaymentVoucherDto dto);
        // ✅ كل Strategy لازم تعمل implement للـ Journal Entry بتاعتها
        protected abstract JournalEntryPostModel BuildJournalEntry(CreatePaymentVoucherDto dto,PaymentVoucher voucher);

        // ✅ كل Strategy لازم تعمل implement للـ Lines بتاعتها
        protected abstract Task BuildVoucherLines(CreatePaymentVoucherDto dto,PaymentVoucher voucher);

        public BasePaymentVoucherStrategy(ApplicationDbContext context, JournalEntryService journalEntryService)
        {
            this.context = context;
            this.journalEntryService = journalEntryService;
        }

        private async Task<List<string>> SharedValidate(CreatePaymentVoucherDto dto)
        {
            var errors = new List<string>();

            if (dto.VoucherDate == DateTime.MinValue)
                errors.Add("VoucherDate is required.");

            if (dto.EmployeeId <= 0)
                errors.Add("EmployeeId is invalid.");
            else if (!ValidateEmployee(dto.EmployeeId))
                errors.Add("Employee does not exist.");

            if (dto.PaymentAccountId <= 0)
                errors.Add("PaymentAccountId is invalid.");
            
            await ValidateAccountForRole(dto.PaymentAccountId, AccountRole.CashBank, errors);

            if (dto.Lines == null || !dto.Lines.Any())
                errors.Add("At least one payment line is required.");
            else if (dto.Lines.Any(l => l.Amount <= 0))
                errors.Add("All line amounts must be greater than zero.");

            if (dto.PaymentMethod == Dto_s.PaymentMethod.Cash)
            {
                if (!dto.CashSessionId.HasValue)
                    errors.Add("Cash payment requires CashSession.");
                else if (!ValidateCashSession(dto.CashSessionId.Value))
                    errors.Add("Cash session does not exist.");
            }

            if (dto.Lines == null || !dto.Lines.Any())
            {
                errors.Add("At least one line is required");
                return errors;
            }
            if (dto.Lines.Any(l => l.Amount <= 0))
                errors.Add("Amount must be positive");

            return errors;
        }

        public async Task<List<string>> ExecuteAsync(CreatePaymentVoucherDto dto)
        {
            var errors = new List<string>();

            var validationResult = await SharedValidate(dto);
            if (validationResult.Count > 0)
                return validationResult;

            try
            {
                // 1. Create Voucher (مشترك بين الكل)
                var paymentVoucher = new PaymentVoucher
                {
                    VoucherDate = dto.VoucherDate,
                    PaymentMethod = (Domain.AccountingEntities.PaymentMethod)dto.PaymentMethod,
                    CashSessionId = dto.CashSessionId,
                    EmployeeId = dto.EmployeeId,
                    IsReversed = false,
                    Status = VoucherStatus.Draft,
                    VoucherNo = await GeneratePaymentVoucherNo(),
                    TotalAmount = dto.Lines.Sum(l => l.Amount),
                    SupplierId = dto.SupplierId,
                };

                // 2. Build Lines (كل Strategy بتعملها بطريقتها)
                BuildVoucherLines(dto, paymentVoucher);

                // 3. Build Journal Entry (كل Strategy بتعملها بطريقتها)
                var journalEntry = BuildJournalEntry(dto, paymentVoucher);

                // 4. Post Journal Entry (مشترك)
                var result = await journalEntryService.PostJournalEntry(journalEntry);
                if (result.Item1 != JournalEntryError.CreatedSuccessfully)
                {
                    errors.Add("Invalid journal Entry operation");
                    return errors;
                }

                // 5. Save Voucher (مشترك)
                paymentVoucher.Status = VoucherStatus.Posted;
                paymentVoucher.JournalEntryId = result.Item2;
                await context.PaymentVouchers.AddAsync(paymentVoucher);
                await context.SaveChangesAsync();

                return errors;
            }
            catch
            {
                errors.Add("Something went wrong");
                return errors;
            }
        }
        protected async Task ValidateAccountForRole(int accountId, AccountRole role, List<string> errors)
        {
            var account = await context.FinancialAccounts
                .Where(a => a.Id == accountId)
                .Select(a => new { a.IsLeaf, a.Type, a.SystemRole })
                .FirstOrDefaultAsync();

            if (account == null)
            {
                errors.Add($"Account {accountId} not found");
                return;
            }

            if (!account.IsLeaf)
                errors.Add($"Account {accountId} must be a leaf");

            var isValid = role switch
            {
                AccountRole.Expense => account.SystemRole == SystemAccountType.Expenses,
                AccountRole.VatRecoverable => account.SystemRole == SystemAccountType.VatInput,
                AccountRole.Cash => account.SystemRole == SystemAccountType.Cash,
                AccountRole.Bank => account.SystemRole == SystemAccountType.Bank,
                AccountRole.CashBank => account.SystemRole == SystemAccountType.Cash
                                     || account.SystemRole == SystemAccountType.Bank,
                AccountRole.Payable => account.SystemRole == SystemAccountType.AccountsPayable,
                AccountRole.AdvancePayment => account.SystemRole == SystemAccountType.SupplierAdvances,
                _ => false
            };

            if (!isValid)
                errors.Add($"Account {accountId} not valid for role {role}");
        }
        public bool ValidateEmployee(int employeeId)
            => context.Employees.Any(e => e.Id == employeeId);
         
        public bool ValidatePaymentAccount(int accountId)
            => context.FinancialAccounts.Any(a => a.Id == accountId);
            
        public bool ValidateCashSession(int cashSessionId)  
            => context.CashSessions.Any(cs => cs.Id == cashSessionId);

        protected async Task<string> GeneratePaymentVoucherNo()
        {
            var nextValue = await context.Database
                .SqlQueryRaw<long>("SELECT NEXT VALUE FOR Fin_PaymentVoucher")
                .FirstAsync();

            var year = DateTime.UtcNow.Year;

            return $"PV-{year}-{nextValue:D6}";
        }
    }
}

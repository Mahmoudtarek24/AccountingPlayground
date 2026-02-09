using AccountingPlayground.Application.Dto_s;
using AccountingPlayground.Application.Enums;
using AccountingPlayground.Application.Interfaces;
using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Domain.Interfaces;
using AccountingPlayground.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AccountingPlayground.Application.Implementation
{
    public class InvoiceServices : IInvoiceServices
    {
        private readonly IFinancialAccountRepository financialAccountRepository;
        private readonly ApplicationDbContext context;
        public InvoiceServices(IFinancialAccountRepository financialAccountRepository,
                               ApplicationDbContext context)
        {
            this.financialAccountRepository = financialAccountRepository;
            this.context = context; 
        }
        public async Task<bool> RegisterInvoice(CreateSupplierInvoice dto)
        {
            if (BasicValidation(dto))
                return false;

            var accountIds = new List<int> { dto.VATAccountId, dto.PayableAccountId, dto.ExpenseAccountId };
            var ValidAccount = await financialAccountRepository.GetValidAnyAccountTypeIdsAsync(accountIds);

            if (ValidAccount.Count != accountIds.Count)
                return false;

            await ValidateAccountForRole(dto.VATAccountId, AccountRole.VatRecoverable);
            await ValidateAccountForRole(dto.ExpenseAccountId, AccountRole.Expense);
            await ValidateAccountForRole(dto.PayableAccountId, AccountRole.Payable);



            return default;
        }

        public async Task ValidateAccountForRole(int accountId, AccountRole role)
        {
            var account = await context.FinancialAccounts.Where(e => e.Id == accountId)
                           .Select(e => new { e.Type, e.IsLeaf }).FirstOrDefaultAsync();

            if(account is null)
                throw new Exception("Account not found");

            if(!account.IsLeaf)
                throw new Exception("Account must be a leaf");

            var IsValid = role switch
            {
                AccountRole.Expense => account.Type == AccountType.Expense,
                AccountRole.Payable => account.Type == AccountType.Liability,
                AccountRole.VatRecoverable => account.Type == AccountType.Asset,
            };
            if(!IsValid)
                throw new Exception($"Account not valid for role {role}");
        }

        public bool BasicValidation(CreateSupplierInvoice dto)
        {
            if(dto.NetAmount>0)
                return true;

            if (dto.VATAmount >= 0)
                return true;

            return false;   
        }
    }
}

using AccountingPlayground.Application.Dto_s;
using AccountingPlayground.Application.Interfaces;
using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Domain.Interfaces;
using AccountingPlayground.Infrastructure.Context;
using AccountingPlayground.Infrastructure.Implementation;

namespace AccountingPlayground.Application.Implementation
{
    public class FinancialAccountService : IFinancialAccountService
    {
        private readonly ApplicationDbContext context;
        private readonly IFinancialAccountRepository financialAccountRepository;
        public FinancialAccountService(IFinancialAccountRepository financialAccountRepository, ApplicationDbContext context)
        {
            this.financialAccountRepository = financialAccountRepository;
            this.context = context;
        }

        public async Task<int> CreateFinancialAccount(CreateFinancialAccountDto dto)
        {
            var FinancialRoot = await financialAccountRepository.GetByIdAsync(dto.ParentId);
            if (FinancialRoot is null  || FinancialRoot.IsLeaf || FinancialRoot.Level==6)
                return 0;
            // fluent validation 

            // name to check same levels name not duplicate 

            FinancialRoot.IsLeaf = false;
            var FinancialAccount = new FinancialAccount
            {
                Name = dto.Name,
                ParentAccountId = FinancialRoot.Id,
                Level = FinancialRoot.Level + 1,
                IsLeaf = true,
                Type = FinancialRoot.Type,
                Code = ""
            };
            // need another endpoint / services 
            // code will assign with null , and will not interact with it until Accountant required endpoint to see account with new code
            // and Accountant can update like 5413 =>5415

            await context.FinancialAccounts.AddAsync(FinancialAccount);
            await context.SaveChangesAsync();       

            return FinancialAccount.Id;
        }

        public async Task<List<FinancialAccount>> GetChartOfAccountsTree() =>
            await financialAccountRepository.GetChartOfAccountsTreeAsync();
    }
}

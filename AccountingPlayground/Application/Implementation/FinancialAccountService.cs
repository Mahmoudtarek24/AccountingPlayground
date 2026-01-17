using AccountingPlayground.Application.Dto_s;
using AccountingPlayground.Application.Interfaces;
using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Domain.Interfaces;
using AccountingPlayground.Infrastructure.Context;
using AccountingPlayground.Infrastructure.Implementation;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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
            if (FinancialRoot is null || FinancialRoot.IsLeaf || FinancialRoot.Level == 6)
                return 0;

            if (!await financialAccountRepository.IsValidName(dto.ParentId, dto.Name))
                return 0;


            FinancialRoot.IsLeaf = false;
            var FinancialAccount = new FinancialAccount
            {
                Name = dto.Name,
                ParentAccountId = FinancialRoot.Id,
                Level = FinancialRoot.Level + 1,
                IsLeaf = true,
                Type = FinancialRoot.Type,
                IsActive = true,
            };
            FinancialAccount.Code = await GenerateCode(FinancialRoot);

            await context.FinancialAccounts.AddAsync(FinancialAccount);
            await context.SaveChangesAsync();

            return FinancialAccount.Id;
        }

        public async Task<List<FinancialAccount>> GetChartOfAccountsTree() =>
            await financialAccountRepository.GetChartOfAccountsTreeAsync();

        public async Task<FinancialAccount> GetById(int id)
        {
            var financialAccount = await financialAccountRepository.GetByIdAsync(id);
            if (financialAccount is null)
                return null;

            return financialAccount;
        }


        private async Task<string> GenerateCode(FinancialAccount parent, AccountType type = AccountType.Asset)
        {
            if (parent.ParentAccountId is null)
                return type switch
                {
                    AccountType.Asset => "1",
                    AccountType.Equity => "2",
                    AccountType.Liability => "3",
                    AccountType.Revenue => "4",
                    AccountType.Expense => "5",
                };

            var siblingCodes = await context.FinancialAccounts
                .Where(e => e.Type == parent.Type && e.Level == parent.Level + 1)
                .Select(e => e.Code)
                .ToListAsync();

            if (!siblingCodes.Any())
                return parent.Code + "1";

            var parentPrefixLength = parent.Code.Length;

            var maxSuffix = siblingCodes
                .Select(code => int.Parse(code.Substring(parentPrefixLength)))
                .Max();

            return parent.Code + (maxSuffix + 1);
        } 
    }
}

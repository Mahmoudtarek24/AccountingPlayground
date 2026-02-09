using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Domain.Interfaces;
using AccountingPlayground.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AccountingPlayground.Infrastructure.Implementation
{
    public class FinancialAccountRepository : IFinancialAccountRepository
    {
        private readonly AccountType[] allowedAccountTypes = new[]
        {
                AccountType.Asset,
                AccountType.Liability,
                AccountType.Equity,
        };  

        private readonly ApplicationDbContext context;
        public FinancialAccountRepository(ApplicationDbContext context)
        {
            this.context = context;     
        }

        public async Task<FinancialAccount?> GetByIdAsync(int Id)
            => await context.FinancialAccounts.FirstOrDefaultAsync(x => x.Id == Id);

        public async Task<bool> IsValidName(int parentAccountId, string name) =>
          !await context.FinancialAccounts.AnyAsync(e =>
                      e.ParentAccountId == parentAccountId && e.Name == name);
    

        public async Task<List<FinancialAccount>> GetChartOfAccountsTreeAsync()
        {
            var accounts = await context.FinancialAccounts.ToListAsync();

            var lookup = accounts.ToLookup(a => a.ParentAccountId);

            foreach (var account in accounts)
            {
                account.Children = lookup[account.Id].ToList();
            }

            return lookup[null].ToList();
        }

        public async Task<List<int>> GetValidAccountIdsAsync(List<int> accountIds)
            => await context.FinancialAccounts
            .Where(e => accountIds.Contains(e.Id)&& allowedAccountTypes.Contains(e.Type)&&e.IsLeaf)
            .Select(a => a.Id).ToListAsync();


        public async Task<List<int>> GetValidAnyAccountTypeIdsAsync(List<int> accountIds)
            => await context.FinancialAccounts
            .Where(e => accountIds.Contains(e.Id) && e.IsLeaf)
            .Select(a => a.Id).ToListAsync();
    }
}

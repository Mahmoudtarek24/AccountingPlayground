using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Domain.Interfaces;
using AccountingPlayground.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AccountingPlayground.Infrastructure.Implementation
{
    public class FinancialAccountRepository : IFinancialAccountRepository
    {
        private readonly ApplicationDbContext context;
        public FinancialAccountRepository(ApplicationDbContext context)
        {
            this.context = context;     
        }

        public async Task<FinancialAccount?> GetByIdAsync(int Id)
        {
            return await context.FinancialAccounts.FirstOrDefaultAsync(x => x.Id == Id);        
        }
        

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
    }
}

using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Domain.Interfaces;
using AccountingPlayground.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AccountingPlayground.Infrastructure.Implementation
{
    public class AccountOpeningBalanceRepository : IAccountOpeningBalanceRepository   
    {
        private readonly ApplicationDbContext context;
        public AccountOpeningBalanceRepository(ApplicationDbContext context)
        {
            this.context = context; 
        }

        public async Task<IEnumerable<FinancialAccount>> GetEligibleAccountsForOpeningBalanceAsync()
        {
            var allowedAccountTypes = new[]
            {
                AccountType.Asset,
                AccountType.Liability,
                AccountType.Equity,
            };          

            var result = await context.FinancialAccounts
                .Where(account => account.IsLeaf && allowedAccountTypes.Contains(account.Type))
                .ToListAsync();  

            return result;  
        }

        public async Task<List<int>> GetValidOpeningBalancesAsync(List<int> accountIds)
            =>   await context.AccountOpeningBalances.Include(e=>e.FinancialAccount)
                .Where(e => accountIds.Contains(e.FinancialAccountId)&&e.FinancialYear.Year==DateTime.Now.Year)
                .Select(ob => ob.FinancialAccountId).ToListAsync(); 
    }
}

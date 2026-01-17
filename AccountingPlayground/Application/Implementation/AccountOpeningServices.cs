using AccountingPlayground.Application.Dto_s;
using AccountingPlayground.Application.Interfaces;
using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Domain.Interfaces;
using AccountingPlayground.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace AccountingPlayground.Application.Implementation
{
    public class AccountOpeningServices : IAccountOpeningServices
    {
        private readonly IFinancialAccountRepository financialAccountRepository;
        private readonly IAccountOpeningBalanceRepository accountOpeningBalanceRepository;
        private readonly IFinancialYearRepository financialYearRepository;
        private readonly ApplicationDbContext context;

        public AccountOpeningServices(
            IFinancialAccountRepository financialAccountRepository,
            IAccountOpeningBalanceRepository accountOpeningBalanceRepository,
            IFinancialYearRepository financialYearRepository,
            ApplicationDbContext context)
        {
            this.financialAccountRepository = financialAccountRepository;
            this.accountOpeningBalanceRepository = accountOpeningBalanceRepository;
            this.financialYearRepository = financialYearRepository;
            this.context = context;
        }

        public async Task<IEnumerable<AccountDto>> GetEligibleAccounts()
        {
            var result = await accountOpeningBalanceRepository.GetEligibleAccountsForOpeningBalanceAsync();

            return result.Select(e => new AccountDto
            {
                Id = e.Id,
                Code = e.Code,
                Name = e.Name,
                Type = e.Type
            }).ToList();
        }



        public async Task<bool> CreateOpeningBalance(CreateOpeningBalanceDto createDto)
        {
            if (!createDto.Items.Any())
                return false;

            //opening year 
            if (await financialYearRepository.IsOpenAsync(DateTime.Now.Year))
                return false;

            
            // check for account is valid "leaf and exist , type asset ,Liability ,Equity" if not valid will return it 
            var accountIds = createDto.Items.Select(e => e.AccountId).Distinct().ToList();
            var validAccount = await financialAccountRepository.GetValidAccountIdsAsync(accountIds);

            var notValidAccountIds = accountIds.Except(validAccount).ToList();      // Will return to user to know that will not apply 

            if (!validAccount.Any())
                return false;

            //مفيش Opening Balance قبل كده
            var validForOpeningBalance = await accountOpeningBalanceRepository.GetValidOpeningBalancesAsync(validAccount);

            var alreadyHaveOpeningBalance = validAccount.Except(validForOpeningBalance).ToList();  // Will return to user to know that will not apply 

            if (validForOpeningBalance.Any())
                return false;


            foreach (var item in validForOpeningBalance)
            {
                var financialAccount = await financialAccountRepository.GetByIdAsync(item);

                var openingBalance = new AccountOpeningBalance
                {
                    FinancialAccountId = item,
                    //FinancialYearId
                };

                switch (financialAccount.Type)
                {
                    case AccountType.Asset:
                        openingBalance.OpeningCredit = 0;
                        openingBalance.OpeningDebit = (long)createDto.Items.Where(e => e.AccountId == item).Sum(e => e.Amount);
                        break;

                    case AccountType.Liability:
                    case AccountType.Equity:
                        openingBalance.OpeningCredit = (long)createDto.Items.Where(e => e.AccountId == item).Sum(e => e.Amount);
                        openingBalance.OpeningDebit = 0;
                        break;

                    default:
                        return false;
                }
                context.AccountOpeningBalances.Add(openingBalance);
            }

            var balancing = context.AccountOpeningBalances.Local.Sum(e => e.OpeningDebit) - context.AccountOpeningBalances.Local.Sum(e => e.OpeningCredit);

            if (balancing != 0)
                return false;

            await context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> CreateOpeningBalances(CreateOpeningBalanceDto createDto)
        {
            if (createDto == null || !createDto.Items.Any())
                return false;

            var year = DateTime.Now.Year;

            var isOpen = await financialYearRepository.IsOpenAsync(year);
            if (!isOpen)
                return false;

            var accountIds = createDto.Items.Select(e => e.AccountId).Distinct().ToList();

            // also need to check that no journal entries exist before allowing opening balance creation

            if (accountIds.Count != createDto.Items.Count)
                return false;

            var validAccountIds = await financialAccountRepository.GetValidAccountIdsAsync(accountIds);

            if (validAccountIds.Count != accountIds.Count)
                return false;

            var alreadyOpenedAccounts = await accountOpeningBalanceRepository.GetValidOpeningBalancesAsync(validAccountIds);

            if (alreadyOpenedAccounts.Any())
                return false;

            var accounts = await context.FinancialAccounts
                          .Where(a => validAccountIds.Contains(a.Id)).ToListAsync();

            var openingBalances = new List<AccountOpeningBalance>();

            foreach (var account in accounts)
            {
                var amount = createDto.Items
                             .Where(e => e.AccountId == account.Id).Sum(e => e.Amount);

                if (amount <= 0)
                    return false;

                var openingBalance = new AccountOpeningBalance
                {
                    FinancialAccountId = account.Id,
                    // FinancialYearId = ...
                    OpeningDebit = 0,
                    OpeningCredit = 0
                };

                switch (account.Type)
                {
                    case AccountType.Asset:
                        openingBalance.OpeningDebit = (long)amount;
                        break;

                    case AccountType.Liability:
                    case AccountType.Equity:
                        openingBalance.OpeningCredit = (long)amount;
                        break;

                    default:
                        return false;
                }

                openingBalances.Add(openingBalance);
            }

            var totalDebit = openingBalances.Sum(e => e.OpeningDebit);
            var totalCredit = openingBalances.Sum(e => e.OpeningCredit);

            if (totalDebit != totalCredit)
                return false;

            using var tx = await context.Database.BeginTransactionAsync();

            try
            {
                context.AccountOpeningBalances.AddRange(openingBalances);
                await context.SaveChangesAsync();
                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

    }
}
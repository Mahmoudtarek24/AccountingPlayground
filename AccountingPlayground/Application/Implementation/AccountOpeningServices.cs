using AccountingPlayground.Application.Adapters;
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


        public async Task<bool> CreateOpeningBalance(CreateOpeningBalanceCommand createDto)
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
                        openingBalance.OpeningDebit = amount;
                    break;

                    case AccountType.Liability:
                    case AccountType.Equity:
                        openingBalance.OpeningCredit = amount;
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

        public async Task<OpeningBalanceResponseDto> GetOpeningBalance(int year)
        {
            var result = await accountOpeningBalanceRepository.GetOpeningBalancesByYearAsync(year);


            return new OpeningBalanceResponseDto
            {
                FinancialYear = year,
                Accounts = result.Select(ob => new OpeningBalanceAccountDto
                {
                    AccountId = ob.FinancialAccount.Id,
                    AccountName = ob.FinancialAccount.Name,
                    Type = ob.FinancialAccount.Type,
                    Debit = ob.OpeningDebit,
                    Credit = ob.OpeningCredit
                }).ToList()
            };
        }

		public void CarryForward(int fromYear, int toYear)
		{
            // first we need to calculate "Net Income"
            // Net Income =  Total Revenues − Total Expenses , calculate it from history of Journal Entries of year

            // first calculate Total Revenues
            // 1 - will get leaf node of Revenue type , then get her Journal Entries

            var revenueLinesByAccount = context.JournalEntryLines
                                        .Include(e => e.JournalEntry)
                                        .Include(e => e.FinancialAccount)
                                        .Where(e =>
                                            e.FinancialAccount.IsLeaf &&
                                            e.FinancialAccount.Type == AccountType.Revenue &&
                                            e.JournalEntry.Date.Year == fromYear
                                        ).GroupBy(e => e.FinancialAccountId)
                                        .Select(e => new
                                        {
                                            AccountId = e.Key,
                                            Lines = e.ToList()
                                        }).ToList();

            long totalDepit = 0;
            long totalCredit = 0;
            long totalRevenue = 0;
            foreach(var calc in revenueLinesByAccount)
            {

                foreach(var cal in calc.Lines)
                {
                    totalCredit += cal.Credit;
                    totalDepit += cal.Debit;

                }
                totalRevenue += totalCredit + totalDepit;
			}


			//long totalRevenue = 0;

			//foreach (var accountGroup in revenueLinesByAccount)
			//{
			//	long accountRevenue = 0;

			//	foreach (var line in accountGroup.Lines)
			//	{
			//		// Revenue = Credit - Debit
			//		accountRevenue += (line.Credit - line.Debit);
			//	}

			//	totalRevenue += accountRevenue;
			//}



		}
	}
}
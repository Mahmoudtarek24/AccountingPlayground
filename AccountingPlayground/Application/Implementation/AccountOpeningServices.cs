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
        public async Task CloseFinancialYear(int fromYear, int toYear)
        {
            var financialYear = await context.FinancialYear.FirstOrDefaultAsync(y => y.Year == fromYear);

            if (financialYear == null || financialYear.IsClosed)
                return;

            // 1- Calculate Net Income
            //OldCodeForCalculateNetIncome();

            var revenuesExpenseLines = await context.JournalEntryLines
                .Include(e => e.FinancialAccount)
                .Include(e => e.FinancialAccount)
                .Where(e => e.FinancialAccount.IsLeaf && e.JournalEntry.Date.Year == fromYear
                 && (e.FinancialAccount.Type == AccountType.Revenue || e.FinancialAccount.Type == AccountType.Expense))
                .Select(e => new
                {
                    e.FinancialAccount.Type,
                    e.Credit,
                    e.Debit,
                }).ToListAsync();

            long totalRevenue = 0;
            long totalExpense = 0;

            foreach(var line in revenuesExpenseLines)
            {
                if(line.Type ==  AccountType.Revenue)
                    totalRevenue += (line.Credit - line.Debit);

                if (line.Type == AccountType.Expense)
                    totalExpense += (line.Debit - line.Credit); 
            }
            var netIncome = totalRevenue - totalExpense;

            //2- Retained Earning

            // need to change hard code to enum , good code it dint tie code with business 
            // her when i tie this condition " e.Name == "Retained Earnings" " now this code is can say not dynamic
            // means if the company need to change this name " e.Name == "Retained Earnings"" will need also to change this code
            // to override this problem will not used enum or hard code , will get type from entity itself replace hard code to name 
            // or code her if name or code change ,It doesn't concern us at all
            //var retainedEarningsAccount = await context.FinancialAccounts
            //                             .FirstOrDefaultAsync(e => e.Type == AccountType.Equity &&
            //                              e.IsLeaf && e.Name == "Retained Earnings" );

            var retainedEarningsAccount = await context.FinancialAccounts
                                         .FirstOrDefaultAsync(e => e.Type == AccountType.Equity &&
                                          e.IsLeaf && e.SystemRole == SystemAccountType.RetainedEarnings);

            if (retainedEarningsAccount is null)
                return;


            // before save it should check have only one on years 

            var hasRetainedEarningsEntryForYear = await context.JournalEntryLines
                          .AnyAsync(e => e.FinancialAccountId == retainedEarningsAccount.Id && 
                          e.JournalEntry.Date >= financialYear.StartDate &&
                          e.JournalEntry.Date <= financialYear.EndDate);
          
            if (hasRetainedEarningsEntryForYear)
                return;

            #region   تسجيل قيد
            var closingEntry = new JournalEntry
            {
                Date = new DateTime(fromYear, 12, 31),
                Reference = $"Year Closing {fromYear}"
            };

            if (netIncome > 0)
                closingEntry.Lines.Add(new JournalEntryLine
                {
                    FinancialAccountId = retainedEarningsAccount.Id,
                    Debit = 0,
                    Credit = netIncome
                });
            
            if (netIncome < 0)
                closingEntry.Lines.Add(new JournalEntryLine
                {
                    FinancialAccountId = retainedEarningsAccount.Id,
                    Debit = netIncome,
                    Credit = 0
                });

            await context.JournalEntries.AddAsync(closingEntry);
            #endregion

            financialYear.IsClosed = true;
            financialYear.ClosedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

        }
        public async Task CarryForwardOpeningBalance(int fromYear, int toYear)
        {
        }
        private void OldCodeForCalculateNetIncome(int fromYear, int toYear)
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

            long totalRevenue = 0;

            foreach (var accountGroup in revenueLinesByAccount)
            {
                long accountRevenue = 0;

                foreach (var line in accountGroup.Lines)
                {
                    // Revenue = Credit - Debit
                    accountRevenue += (line.Credit - line.Debit);
                }

                totalRevenue += accountRevenue;
            }
            // will get total Expenses 


            var expensesLinesByAccount = context.JournalEntryLines.Include(e => e.FinancialAccount).Include(e => e.JournalEntry)
                 .Where(e => e.FinancialAccount.IsLeaf && e.FinancialAccount.Type == AccountType.Expense
                       && e.JournalEntry.Date.Year == fromYear).Select(e => new
                       {
                           Credit = e.Credit,
                           Debit = e.Debit,
                       }).ToList();

            long totalExpenses = 0;

            foreach (var accountGroup in expensesLinesByAccount)
                totalExpenses += (accountGroup.Debit - accountGroup.Credit);

            var netIncome = totalRevenue - totalExpenses;
        }

    }
}
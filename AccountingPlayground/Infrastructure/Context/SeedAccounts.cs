using AccountingPlayground.Domain.AccountingEntities;

namespace AccountingPlayground.Infrastructure.Context
{
    public static class SeedAccounts
    {
        public static async Task SeedAllAccountsAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await SeedAssetAccountsAsync(context);
            await SeedLiabilityAccountsAsync(context);
            await SeedRevenueAndExpenseAccountsAsync(context);
            await SeedEquityAccountsAsync(context);
        } 
        public static async Task SeedAssetAccountsAsync(ApplicationDbContext context)
        {
            if (context.FinancialAccounts.Any(a => a.Type == AccountType.Asset))
                return;

            // Root
            var assets = CreateAccount("Assets","1",1,false,AccountType.Asset);

            context.FinancialAccounts.Add(assets);
            await context.SaveChangesAsync();

            // Cash
            var cash = CreateAccount("Cash","11",2,false,AccountType.Asset,assets.Id);

            context.FinancialAccounts.Add(cash);
            await context.SaveChangesAsync();

            // Cash children
            context.FinancialAccounts.AddRange(
                CreateAccount("Main Cash", "1101", 3, true, AccountType.Asset, cash.Id),
                CreateAccount("Petty Cash", "1102", 3, true, AccountType.Asset, cash.Id)
            );

            await context.SaveChangesAsync();

            // Bank
            var bank = CreateAccount("Bank", "12", 2, false, AccountType.Asset, assets.Id);

            context.FinancialAccounts.Add(bank);
            await context.SaveChangesAsync();

            // Bank children
            context.FinancialAccounts.AddRange(
                CreateAccount("National Bank of Egypt", "1201", 3, true, AccountType.Asset, bank.Id),
                CreateAccount("Banque Misr", "1202", 3, true, AccountType.Asset, bank.Id)
            );

            await context.SaveChangesAsync();

            // Inventory
            var inventory = CreateAccount("Inventory", "13", 2, false, AccountType.Asset, assets.Id);

            context.FinancialAccounts.Add(inventory);
            await context.SaveChangesAsync();

            context.FinancialAccounts.Add(
                CreateAccount("Raw Materials Inventory", "1301", 3, true, AccountType.Asset, inventory.Id)
            );

            // Receivables
            context.FinancialAccounts.AddRange(
                CreateAccount("Accounts Receivable", "14", 2, true, AccountType.Asset, assets.Id),
                CreateAccount("Employee Receivable", "15", 2, true, AccountType.Asset, assets.Id)
            );

            await context.SaveChangesAsync();
        }
        public static async Task SeedLiabilityAccountsAsync(ApplicationDbContext context)
        {
            if (context.FinancialAccounts.Any(a => a.Type == AccountType.Liability))
                return;

            // Root
            var liabilities = CreateAccount("Liabilities", "2", 1, false, AccountType.Liability);

            context.FinancialAccounts.Add(liabilities);
            await context.SaveChangesAsync();

            // Children (Leaf accounts)
            context.FinancialAccounts.AddRange(
                CreateAccount("Accounts Payable", "21", 2, true, AccountType.Liability,liabilities.Id),
                CreateAccount("Employees Payable", "22", 2, true, AccountType.Liability, liabilities.Id),
                CreateAccount("VAT Payable", "23", 2, true, AccountType.Liability, liabilities.Id),
                CreateAccount("Income Tax Payable", "24", 2, true, AccountType.Liability, liabilities.Id),
                CreateAccount("Bank Loans", "25", 2, true, AccountType.Liability, liabilities.Id)
            );

            await context.SaveChangesAsync();
        }
        public static async Task SeedRevenueAndExpenseAccountsAsync(ApplicationDbContext context)
        {
            if (context.FinancialAccounts.Any(a =>
                a.Type == AccountType.Revenue || a.Type == AccountType.Expense))
                return;

            // ---------- Revenue ----------
            var revenue = CreateAccount("Revenue","4",1,false,AccountType.Revenue);

            context.FinancialAccounts.Add(revenue);
            await context.SaveChangesAsync();

            context.FinancialAccounts.AddRange(
                CreateAccount("Food Sales", "41", 2, true, AccountType.Revenue, revenue.Id),
                CreateAccount("Drinks Sales", "42", 2, true, AccountType.Revenue, revenue.Id),
                CreateAccount("Service Revenue", "43", 2, true, AccountType.Revenue, revenue.Id)
            );

            await context.SaveChangesAsync();

            // ---------- Expenses ----------
            var expenses = CreateAccount("Expenses","5",1,false,AccountType.Expense);

            context.FinancialAccounts.Add(expenses);
            await context.SaveChangesAsync();

            context.FinancialAccounts.AddRange(
                CreateAccount("Cost of Goods Sold (COGS)", "51", 2, true, AccountType.Expense, expenses.Id),
                CreateAccount("Salaries Expense", "52", 2, true, AccountType.Expense, expenses.Id),
                CreateAccount("Rent Expense", "53", 2, true, AccountType.Expense, expenses.Id),
                CreateAccount("Utilities Expense", "54", 2, true, AccountType.Expense, expenses.Id),
                CreateAccount("Maintenance Expense", "55", 2, true, AccountType.Expense, expenses.Id),
                CreateAccount("Cash Shortage Expense", "56", 2, true, AccountType.Expense, expenses.Id)
            );

            await context.SaveChangesAsync();
        }   
        public static async Task SeedEquityAccountsAsync(ApplicationDbContext context)
        {
            if (context.FinancialAccounts.Any(a => a.Type == AccountType.Equity))
                return;

            var equity = CreateAccount("Equity", "3", 1, false, AccountType.Equity);

            context.FinancialAccounts.Add(equity);
            await context.SaveChangesAsync();

            // Children (Leaf)
            context.FinancialAccounts.AddRange(
                CreateAccount("Capital", "31", 2, true, AccountType.Equity, equity.Id),
                CreateAccount("Retained Earnings","32", 2,true,AccountType.Equity,equity.Id, SystemAccountType.RetainedEarnings)
            );

            await context.SaveChangesAsync();
        }

        private static FinancialAccount CreateAccount(string name, string code, int level, bool isLeaf,
                                                      AccountType type,int? parentId = null ,
                                                      SystemAccountType systemAccount = SystemAccountType.None) 
            =>  new FinancialAccount
            {
                Name = name,
                Code = code,
                Level = level,
                IsLeaf = isLeaf,
                Type = type,
                ParentAccountId = parentId,
                SystemRole = systemAccount
            };
        
    }

}

using AccountingPlayground.Domain.AccountingEntities;

namespace AccountingPlayground.Infrastructure.Context
{
	public static class SeedAccounts
	{
		public static async Task SeedAssetAccountsAsync(IServiceProvider serviceProvider)
		{
			using var scope = serviceProvider.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			if (context.FinancialAccounts.Any(a => a.Type == AccountType.Asset))
				return;

			var assets = new FinancialAccount
			{
				Name = "Assets",
				Type = AccountType.Asset
			};

			context.FinancialAccounts.Add(assets);
			await context.SaveChangesAsync();

			var cash = new FinancialAccount
			{
				Name = "Cash",
				Type = AccountType.Asset,
				ParentAccountId = assets.Id
			};

			context.FinancialAccounts.Add(cash);
			await context.SaveChangesAsync();

			context.FinancialAccounts.AddRange(
				new FinancialAccount
				{
					Name = "Main Cash",
					Type = AccountType.Asset,
					ParentAccountId = cash.Id
				},
				new FinancialAccount
				{
					Name = "Petty Cash",
					Type = AccountType.Asset,
					ParentAccountId = cash.Id
				}
			);

			var bank = new FinancialAccount
			{
				Name = "Bank",
				Type = AccountType.Asset,
				ParentAccountId = assets.Id
			};

			context.FinancialAccounts.Add(bank);
			await context.SaveChangesAsync();

			context.FinancialAccounts.AddRange(
				new FinancialAccount
				{
					Name = "National Bank of Egypt",
					Type = AccountType.Asset,
					ParentAccountId = bank.Id
				},
				new FinancialAccount
				{
					Name = "Banque Misr",
					Type = AccountType.Asset,
					ParentAccountId = bank.Id
				}
			);

			var inventory = new FinancialAccount
			{
				Name = "Inventory",
				Type = AccountType.Asset,
				ParentAccountId = assets.Id
			};

			context.FinancialAccounts.Add(inventory);
			await context.SaveChangesAsync();

			context.FinancialAccounts.Add(
				new FinancialAccount
				{
					Name = "Raw Materials Inventory",
					Type = AccountType.Asset,
					ParentAccountId = inventory.Id
				}
			);

			context.FinancialAccounts.Add(
				new FinancialAccount
				{
					Name = "Accounts Receivable",
					Type = AccountType.Asset,
					ParentAccountId = assets.Id
				}
			);

			context.FinancialAccounts.Add(
				new FinancialAccount
				{
					Name = "Employee Receivable",
					Type = AccountType.Asset,
					ParentAccountId = assets.Id
				}
			);

			await context.SaveChangesAsync();
		}
		public static async Task SeedLiabilityAccountsAsync(IServiceProvider serviceProvider)
		{
			using var scope = serviceProvider.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			if (context.FinancialAccounts.Any(a => a.Type == AccountType.Liability))
				return;

			var liabilities = new FinancialAccount
			{
				Name = "Liabilities",
				Type = AccountType.Liability
			};

			context.FinancialAccounts.Add(liabilities);
			await context.SaveChangesAsync();

			context.FinancialAccounts.AddRange(
				new FinancialAccount
				{
					Name = "Accounts Payable",
					Type = AccountType.Liability,
					ParentAccountId = liabilities.Id
				},
				new FinancialAccount
				{
					Name = "Employees Payable",
					Type = AccountType.Liability,
					ParentAccountId = liabilities.Id
				},
				new FinancialAccount
				{
					Name = "VAT Payable",
					Type = AccountType.Liability,
					ParentAccountId = liabilities.Id
				},
				new FinancialAccount
				{
					Name = "Income Tax Payable",
					Type = AccountType.Liability,
					ParentAccountId = liabilities.Id
				},
				new FinancialAccount
				{
					Name = "Bank Loans",
					Type = AccountType.Liability,
					ParentAccountId = liabilities.Id
				}
			);

			await context.SaveChangesAsync();
		}
		public static async Task SeedRevenueAndExpenseAccountsAsync(IServiceProvider serviceProvider)
		{
			using var scope = serviceProvider.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			if (context.FinancialAccounts.Any(a =>
				a.Type == AccountType.Revenue || a.Type == AccountType.Expense))
				return;

			// Revenue 
			var revenue = new FinancialAccount
			{
				Name = "Revenue",
				Type = AccountType.Revenue
			};

			context.FinancialAccounts.Add(revenue);
			await context.SaveChangesAsync();

			context.FinancialAccounts.AddRange(
				new FinancialAccount
				{
					Name = "Food Sales",
					Type = AccountType.Revenue,
					ParentAccountId = revenue.Id
				},
				new FinancialAccount
				{
					Name = "Drinks Sales",
					Type = AccountType.Revenue,
					ParentAccountId = revenue.Id
				},
				new FinancialAccount
				{
					Name = "Service Revenue",
					Type = AccountType.Revenue,
					ParentAccountId = revenue.Id
				}
			);

			var expenses = new FinancialAccount
			{
				Name = "Expenses",
				Type = AccountType.Expense
			};

			context.FinancialAccounts.Add(expenses);
			await context.SaveChangesAsync();

			context.FinancialAccounts.AddRange(
				new FinancialAccount
				{
					Name = "Cost of Goods Sold (COGS)",
					Type = AccountType.Expense,
					ParentAccountId = expenses.Id
				},
				new FinancialAccount
				{
					Name = "Salaries Expense",
					Type = AccountType.Expense,
					ParentAccountId = expenses.Id
				},
				new FinancialAccount
				{
					Name = "Rent Expense",
					Type = AccountType.Expense,
					ParentAccountId = expenses.Id
				},
				new FinancialAccount
				{
					Name = "Utilities Expense",
					Type = AccountType.Expense,
					ParentAccountId = expenses.Id
				},
				new FinancialAccount
				{
					Name = "Maintenance Expense",
					Type = AccountType.Expense,
					ParentAccountId = expenses.Id
				},
				new FinancialAccount
				{
					Name = "Cash Shortage Expense",
					Type = AccountType.Expense,
					ParentAccountId = expenses.Id
				}
			);

			await context.SaveChangesAsync();
		}
		public static async Task SeedEquityAccountsAsync(IServiceProvider serviceProvider)
		{
			using var scope = serviceProvider.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			if (context.FinancialAccounts.Any(a =>
				a.Name == "Equity" && a.Type == AccountType.Equity))
				return;

			var equity = new FinancialAccount
			{
				Name = "Equity",
				Type = AccountType.Equity
			};

			context.FinancialAccounts.Add(equity);
			await context.SaveChangesAsync();

			context.FinancialAccounts.AddRange(
				new FinancialAccount
				{
					Name = "Capital",
					Type = AccountType.Equity,
					ParentAccountId = equity.Id
				},
				new FinancialAccount
				{
					Name = "Retained Earnings",
					Type = AccountType.Equity,
					ParentAccountId = equity.Id
				}
			);

			await context.SaveChangesAsync();
		}

	}

}

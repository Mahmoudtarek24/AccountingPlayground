using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountingPlayground.Infrastructure.Context
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(
				typeof(ApplicationDbContext).Assembly
			);
		}

		// Accounting 

		public DbSet<FinancialAccount> FinancialAccounts { get; set; }
		public DbSet<AccountOpeningBalance> AccountOpeningBalances { get; set; }
		public DbSet<JournalEntry> JournalEntries { get; set; }
		public DbSet<JournalEntryLine> JournalEntryLines { get; set; }
		public DbSet<CashSession> CashSessions { get; set; }
		public DbSet<PaymentVoucher> PaymentVouchers { get; set; }
		public DbSet<ReceiptVoucher> ReceiptVouchers { get; set; }
		public DbSet<Expense> Expenses { get; set; }
		public DbSet<IncomeTax> IncomeTaxes { get; set; }
		public DbSet<VatReturn> VatReturns { get; set; }


		public DbSet<Employee> Employees { get; set; }
		public DbSet<Customer> Customers { get; set; }
		public DbSet<Supplier> Suppliers { get; set; }
		public DbSet<SalesInvoice> SalesInvoices { get; set; }
		public DbSet<SalesInvoiceLine> SalesInvoiceLines { get; set; }
		public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }
		public DbSet<PurchaseInvoiceLine> PurchaseInvoiceLines { get; set; }
		public DbSet<PurchaseReturn> PurchaseReturns { get; set; }
		public DbSet<PurchaseReturnLine> PurchaseReturnLines { get; set; }
		public DbSet<MenuItem> MenuItems { get; set; }
		public DbSet<Ingredient> Ingredients { get; set; }
		public DbSet<RecipeLine> RecipeLines { get; set; }

	}
}

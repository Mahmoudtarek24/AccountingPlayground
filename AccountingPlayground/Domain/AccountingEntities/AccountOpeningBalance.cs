namespace AccountingPlayground.Domain.AccountingEntities
{
	public class AccountOpeningBalance
	{ 
		public int Id { get; set; }

		public int FinancialAccountId { get; set; }
		public FinancialAccount FinancialAccount { get; set; }

		public DateTime OpeningDate { get; set; }
		public long OpeningDebit { get; set; }
		public long OpeningCredit { get; set; }
	}
}
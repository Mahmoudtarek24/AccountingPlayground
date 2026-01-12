namespace AccountingPlayground.Domain.AccountingEntities
{
	public class FinancialAccount
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public AccountType Type { get; set; }
        public string Code { get; set; }
        public int Level { get; set; }
        public bool IsLeaf { get; set; } // true only for leaf node have "doubly entry"

        public int? ParentAccountId { get; set; }
		public FinancialAccount ParentAccount { get; set; }		
		public ICollection<FinancialAccount> Children { get; set; }

		public  ICollection<AccountOpeningBalance> OpeningBalance { get; set; }

	}

	public enum AccountType
	{
		Asset = 1,
		Liability = 2,
		Equity = 3,
		Revenue = 4,
		Expense = 5
	}
}

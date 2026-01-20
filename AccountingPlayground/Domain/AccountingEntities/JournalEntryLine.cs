namespace AccountingPlayground.Domain.AccountingEntities
{
	public class JournalEntryLine
	{
		public int Id { get; set; }  
		 
		public int JournalEntryId { get; set; }
		public JournalEntry JournalEntry { get; set; }

		public int FinancialAccountId { get; set; }
		public FinancialAccount FinancialAccount { get; set; }

		public long Debit { get; set; }    
		public long Credit { get; set; }   
	}
}

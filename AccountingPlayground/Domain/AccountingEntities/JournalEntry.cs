namespace AccountingPlayground.Domain.AccountingEntities
{
	public class JournalEntry
	{
		public int Id { get; set; }
		public DateTime Date { get; set; }
		public string Reference { get; set; }     // SalesInvoice #5, Payment #3

		public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();

        public int FinancialYearId { get; set; }
		public FinancialYear FinancialYear { get; set; }
    }
}

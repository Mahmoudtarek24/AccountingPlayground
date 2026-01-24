namespace AccountingPlayground.Application.Dto_s
{
    public class JournalEntryPostModel
    {
        public DateTime EntryDate { get; set; }
        public string Reference { get; set; } = string.Empty; 
        public IReadOnlyCollection<JournalEntryLinePostModel> Lines { get; set; } // IReadOnlyCollection to prevent update after create 
            = new List<JournalEntryLinePostModel>();
    }

    public class JournalEntryLinePostModel
    {
        public int FinancialAccountId { get; set; }
        public long Debit { get; set; }
        public long Credit { get; set; }
    }
}

namespace AccountingPlayground.Domain.AccountingEntities
{
    public class FinancialYear
    {
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsClosed { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}

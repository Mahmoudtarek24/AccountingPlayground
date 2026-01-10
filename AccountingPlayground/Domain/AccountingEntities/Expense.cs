namespace AccountingPlayground.Domain.AccountingEntities
{
	public class Expense // المصروفات 
	{
		public int Id { get; set; }
		public DateTime Date { get; set; }

		public long Amount { get; set; }          // بالقرش
		public string Type { get; set; }          // Rent, Salary, Electricity
	}
}

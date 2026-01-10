using AccountingPlayground.Domain.AccountingEntities;

namespace AccountingPlayground.Domain.Entities
{
	public class Employee
	{
		public int Id { get; set; }	
		public string Name { get; set; }
		public CashSession CashSession { get; set; }	
	}
}

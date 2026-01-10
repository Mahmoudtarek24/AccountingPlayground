using AccountingPlayground.Domain.Entities;

namespace AccountingPlayground.Domain.AccountingEntities
{
	public class CashSession
	{
		public int Id { get; set; }

		public int EmployeeId { get; set; }
		public Employee Employee { get; set; }

		public DateTime OpenedAt { get; set; }
		public DateTime? ClosedAt { get; set; }

		public long OpeningCash { get; set; }   // فلوس أول الشيفت
		public long? ClosingCash { get; set; }  // اللي اتعدّت آخر الشيفت
	}
}

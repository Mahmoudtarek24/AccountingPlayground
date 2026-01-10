using AccountingPlayground.Domain.AccountingEntities.Enums;
using AccountingPlayground.Domain.Entities;

namespace AccountingPlayground.Domain.AccountingEntities
{
	public class PaymentVoucher
	{
		public int Id { get; set; }	
		public string VoucherNo { get; set; }	
		public DateTime VoucherDate { get; set; }
		public PaymentMethod PaymentMethod { get; set; }
		public long Amount { get; set; }
		public PaymentReferenceType ReferenceType { get; set; }
		public int ReferenceId { get; set; }   // Supplier / Tax / Expense


		public int EmployeeId { get; set; }
		public Employee Employee { get; set; }

		public int? CashSessionId { get; set; }     // 👈 اختياري
		public CashSession? CashSession { get; set; }
	}
}

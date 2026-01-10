using AccountingPlayground.Domain.AccountingEntities.Enums;
using AccountingPlayground.Domain.Entities;

namespace AccountingPlayground.Domain.AccountingEntities
{
	public class ReceiptVoucher
	{
		public int Id { get; set; }
		public string VoucherNo { get; set; }
		public DateTime VoucherDate { get; set; }
		public PaymentMethod PaymentMethod { get; set; }
		public long Amount { get; set; }

		public ReceiptReferenceType ReferenceType { get; set; }
		public int ReferenceId { get; set; }   // CustomerId

		public int EmployeeId { get; set; }
		public Employee Employee { get; set; }

		public int? CashSessionId { get; set; }     // 👈 اختياري
		public CashSession? CashSession { get; set; }

	}
}

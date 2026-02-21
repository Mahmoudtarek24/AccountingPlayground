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
		//public PaymentReferenceType ReferenceType { get; set; }
		//public int ReferenceId { get; set; }   // Supplier / Tax / Expense

		public long NetAmount { get; set; }      // المصروف
		public long VatAmount { get; set; }      // الضريبة
		public long TotalAmount { get; set; }    // اللي اتدفع فعليًا

		public int EmployeeId { get; set; } //	الموضف الي طلع الفلوس واستلم PaymentVoucher
		public Employee Employee { get; set; }

		public int? CashSessionId { get; set; }    
		public CashSession? CashSession { get; set; }

		public ICollection<PaymentVoucherLine> PaymentVoucherLines { get; set; }

        public int JournalEntryId { get; set; }
        public JournalEntry JournalEntry { get; set; }

		public bool IsReversed { get; set; } // indicate if this voucher has been reversed
    }
	public enum PaymentMethod
	{
		Cash = 1,
		Bank = 2
	}

	public enum PaymentReferenceType
	{
		Supplier = 1,
		Tax = 2,
		Expense = 3
	}
}

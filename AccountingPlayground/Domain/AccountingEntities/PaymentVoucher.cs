using AccountingPlayground.Domain.AccountingEntities.Enums;
using AccountingPlayground.Domain.Entities;

namespace AccountingPlayground.Domain.AccountingEntities
{
    public partial class PaymentVoucher
    {
        public int Id { get; set; }
        public string VoucherNo { get; set; } 
        public DateTime VoucherDate { get; set; }
        public VoucherStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        // === Totals (calculated from lines) ===
        public decimal TotalAmount { get; set; }


        // === Supplier (optional - مش كل voucher لازم يكون لمورد) ===
        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        // === Employee & Cash Session ===
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public int? CashSessionId { get; set; }
        public CashSession? CashSession { get; set; }

        // === Journal Entry ===
        public int? JournalEntryId { get; set; }  // if nullable will create payment voucher with draft status 
        public JournalEntry? JournalEntry { get; set; }

        // === Reversal ===
        public bool IsReversed { get; set; }
        public int? OriginalVoucherId { get; set; } 
        public PaymentVoucher? OriginalVoucher { get; set; }
        
        // === Lines ===
        public ICollection<PaymentVoucherLine> Lines { get; set; } = new List<PaymentVoucherLine>();
    }

	public enum PaymentMethod
	{
		Cash = 1,
		Bank = 2
	}
    public enum VoucherStatus
    {
        Draft = 1, // Journal Entry not generated can be edit or delete
        Posted = 2, // Journal Entry generated can not be edit or delete , used revers on  modify 
        Reversed = 3 
    }
}

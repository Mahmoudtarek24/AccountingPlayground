namespace AccountingPlayground.Domain.AccountingEntities
{
	public class PaymentVoucherLine
	{
		public int Id { get; set; }

		public int PaymentVoucherId { get; set; }

		public int FinancialAccountId { get; set; } // Expense / Supplier
		public long Amount { get; set; }

		public ICollection<PaymentVoucher> paymentVouchers { get; set; }	
	}
}

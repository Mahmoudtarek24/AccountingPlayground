namespace AccountingPlayground.Domain.Entities
{
	public class PurchaseReturn
	{
		public int Id { get; set; }
		public DateTime Date { get; set; }

		public long NetAmount { get; set; }   // بالقرش
		public long VatAmount { get; set; }   // بالقرش

		public int SupplierId { get; set; }
		public Supplier Supplier { get; set; }

		public int PurchaseInvoiceId { get; set; }   // الفاتورة الأصلية

		public List<PurchaseReturnLine> Lines { get; set; } = new();
	}
}

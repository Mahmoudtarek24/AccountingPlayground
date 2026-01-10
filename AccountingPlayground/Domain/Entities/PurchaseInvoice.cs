namespace AccountingPlayground.Domain.Entities
{
	public class PurchaseInvoice
	{
		public int Id { get; set; }
		public DateTime Date { get; set; }

		public long NetAmount { get; set; }
		public long VatAmount { get; set; }

		public int SupplierId { get; set; }
		public Supplier Supplier { get; set; }

		public List<PurchaseInvoiceLine> Lines { get; set; } = new();
	}
}

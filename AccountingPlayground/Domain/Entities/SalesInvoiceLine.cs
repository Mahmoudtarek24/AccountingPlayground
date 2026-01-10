namespace AccountingPlayground.Domain.Entities
{
	public class SalesInvoiceLine
	{
		public int Id { get; set; }

		public int SalesInvoiceId { get; set; }
		public SalesInvoice SalesInvoice { get; set; }

		public int MenuItemId { get; set; }
		public MenuItem MenuItem { get; set; }

		public decimal Quantity { get; set; }
		public long UnitPrice { get; set; }
	}
}

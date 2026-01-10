namespace AccountingPlayground.Domain.Entities
{
	public class PurchaseInvoiceLine
	{
		public int Id { get; set; }

		public int PurchaseInvoiceId { get; set; }
		public PurchaseInvoice PurchaseInvoice { get; set; }

		public int IngredientId { get; set; }
		public Ingredient Ingredient { get; set; }

		public decimal Quantity { get; set; }
		public long UnitPrice { get; set; }

		public decimal NetAmount => Quantity * UnitPrice;
	}
}

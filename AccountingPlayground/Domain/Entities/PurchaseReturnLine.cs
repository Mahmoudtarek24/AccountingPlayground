namespace AccountingPlayground.Domain.Entities
{
	public class PurchaseReturnLine
	{
		public int Id { get; set; }

		public int PurchaseReturnId { get; set; } 
		public PurchaseReturn PurchaseReturn { get; set; }

		public int IngredientId { get; set; }
		public Ingredient Ingredient { get; set; }

		public decimal Quantity { get; set; }
		public long UnitPrice { get; set; }
	}
}
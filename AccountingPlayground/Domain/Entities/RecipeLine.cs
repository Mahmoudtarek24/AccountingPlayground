using Microsoft.EntityFrameworkCore;

namespace AccountingPlayground.Domain.Entities
{
	[PrimaryKey(nameof(MenuItemId), nameof(IngredientId))]
	public class RecipeLine
	{
		public int MenuItemId { get; set; }
		public MenuItem MenuItem { get; set; }

		public int IngredientId { get; set; }
		public Ingredient Ingredient { get; set; }

		public decimal QuantityUsed { get; set; }
	}
}

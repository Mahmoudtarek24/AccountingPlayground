using AccountingPlayground.Domain.AccountingEntities;
using Microsoft.EntityFrameworkCore;

namespace AccountingPlayground.Domain.Entities
{
	public class Customer
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}

using AccountingPlayground.Domain.AccountingEntities.Enums;
using AccountingPlayground.Domain.Entities;

namespace AccountingPlayground.Domain.AccountingEntities
{
	public class VatReturn
	{
		public int Id { get; set; }

		public DateTime From { get; set; }
		public DateTime To { get; set; }

		public long InputVat { get; set; }        // بالقرش
		public long OutputVat { get; set; }       // بالقرش
	}
}
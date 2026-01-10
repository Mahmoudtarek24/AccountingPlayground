namespace AccountingPlayground.Domain.Entities
{
	public class SalesInvoice
	{
		public int Id { get; set; }
		public DateTime Date { get; set; }

		public long NetAmount { get; set; }   
		public long VatAmount { get; set; }  

		public int CustomerId { get; set; }
		public Customer Customer { get; set; }

		public List<SalesInvoiceLine> Lines { get; set; } = new();
	}
}

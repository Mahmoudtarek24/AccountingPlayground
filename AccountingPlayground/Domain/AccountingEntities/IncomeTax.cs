namespace AccountingPlayground.Domain.AccountingEntities
{
	public class IncomeTax //تتحسب على صافي الربح السنوي
	{
		public int Id { get; set; }	
		public DateTime From { get; set; }
		public DateTime To { get; set; }

		public decimal TaxRate { get; set; }      // egy 22.5%
		public long TaxableProfit { get; set; }   // صافي الربح (بالقرش)
		public long TaxAmount { get; set; }       // قيمة الضريبة (بالقرش)
	}
}
// ضريبة الدخل مش علي فتوره او عمليه او شخص 
// هي على نتيجة فترة كاملة    // no need to tie or make relation with another model 


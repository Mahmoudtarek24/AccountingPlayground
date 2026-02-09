namespace AccountingPlayground.Domain.Entities
{
	public class Supplier
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string? TaxNumber { get; set; }	
		public TaxRegistrationStatus  TaxRegistrationStatus { get; set; }
		public bool IsSubjectToWithholding { get; set; }		
    }
	public enum TaxRegistrationStatus
	{
        Registered,
		NotRegistered,
		Exempt,
    }
}
// should for supplier store to it 
// 1 -Tax Registration Status    based on this value if is register on "مصلحة الضرايب" apply normal WithholdingTax , if not register will apply higher WithholdingTax 
// 2- Subject To Withholding
// 3- activity , to can now her value need to apply of WithholdingTax
// and another filed related to مصلحة الضرايب 


// الكلام ده ده ليه غلاقع اقو ب انشاء الفاتوره من payment voucher 
namespace AccountingPlayground.Domain.AccountingEntities
{
    public class FinancialSettings
    {
        public int? ServiceFeeType { get; set; }
        public decimal ServiceFee { get; set; }
    }
    public class TaxSetting
    {
        public int Id { get; set; } 
        public string Key { get; set; } 
        public string Value { get; set; }   
    }
}

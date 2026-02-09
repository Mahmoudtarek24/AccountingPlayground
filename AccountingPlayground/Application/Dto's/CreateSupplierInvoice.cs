namespace AccountingPlayground.Application.Dto_s
{
    public class CreateSupplierInvoice  //////  ده هنستحدمه في سيناريو اني سجلت فاتوره وعليا فلوس 
    {
        public DateTime InvoiceDate { get; set; }   
        public int Supplier {  get; set; }   //  who will Recognition  i have money to him 
        public decimal NetAmount { get; set; }  
        public decimal VATAmount { get; set; }


        // Accounts 
        public int VATAccountId {  get; set; }    
        public int PayableAccountId { get; set; } 
        public int ExpenseAccountId { get; set; } 
    }
}

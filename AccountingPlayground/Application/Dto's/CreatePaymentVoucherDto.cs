namespace AccountingPlayground.Application.Dto_s
{
    public class CreatePaymentVoucherDto
    {
        public DateTime VoucherDate { get; set; }                                  // global validation  
        public PaymentMethod PaymentMethod { get; set; }  // Cash or Bank          // global validation , cash && CashSessionId is null => return false 
        public SettlementType VoucherType { get; set; }
        public string? Description { get; set; }

        // ✅ الحساب اللي الفلوس هتطلع منه
        public int PaymentAccountId { get; set; }                                   // global validation

        public int? SupplierId { get; set; }

        // الموظف والكاشير
        public int EmployeeId { get; set; }                                         // global validation
        public int? CashSessionId { get; set; }                                     // global validation

        // الأسطر - هنا كل السحر
        public List<PaymentVoucherLineDto> Lines { get; set; }                     // global validation can not be null 
    }

    public class PaymentVoucherLineDto
    {
        public int? FinancialAccountId { get; set; }   // حساب المصروف (null لو دي Line ضريبة)
        public int? VatAccountId { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }            // المبلغ الأساسي
        public int? PurchaseInvoiceId { get; set; }        // ربط بفاتورة  "الفاتوره الي مفروض دين عليا وانا هسددها "

    }
    public enum SettlementType
    {
        DirectExpense = 1,        // مصروف مباشر
        PayableSettlement = 2,    // سداد فاتورة (full أو partial - الـ Service هيحدد)
        AdvancePayment = 3,       // دفعة مقدمة
        Overpayment = 4,          // دفع زيادة متعمد
        WithholdingTax = 5,       // خصم من المنبع
        Reversal = 6              // قيد عكسي
    }
    public enum PaymentMethod
    {
        Bank,
        Cash,
    }
}

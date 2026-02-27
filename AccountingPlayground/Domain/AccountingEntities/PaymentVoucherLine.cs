using AccountingPlayground.Domain.Entities;

namespace AccountingPlayground.Domain.AccountingEntities
{ 
    public class PaymentVoucherLine
    {
        public int Id { get; set; }
        public int PaymentVoucherId { get; set; }
        public PaymentVoucher PaymentVoucher { get; set; }

        public int FinancialAccountId { get; set; }
        public FinancialAccount FinancialAccount { get; set; }

        public decimal Amount { get; set; }      // المبلغ الأساسي
        public decimal? VatAmount { get; set; }   // الضريبة على السطر ده
        public decimal TotalAmount { get; set; }


        // === Invoice Reference (للربط بفاتورة مورد) ===
        //تخيل إنك بتدفع للمورد **15,000 جنيه** بتسدد بيها **3 فواتير** مع بعض:
        public int? PurchaseInvoiceId { get; set; } 
        public PurchaseInvoice? PurchaseInvoice { get; set; }

        //public int? VatAccountId { get; set; }  // حساب الضريبة
        //public FinancialAccount? VatAccount { get; set; }
    }
}

namespace AccountingPlayground.Application.Dto_s
{
    public class CreatePaymentVoucherDto
    {
        public DateTime VoucherDate { get; set; } 
        public PaymentMethod PaymentMethod { get; set; }
        public int EmployeeId { get; set; } 
        public int? CashSessionId { get; set; }

        // Payment
        public SettlementType SettlementType { get; set; }


        // Payment info (ALWAYS USED)
        public decimal PaidAmount { get; set; } // is named also total ,GrossAmount
        public int CashAccountId { get; set; }// need to make it nullable
        public int? BankAccountId { get; set; }

        // Scenario A: Cash Expense + VAT
        public decimal NetAmount { get; set; } // need to make it nullable
        public decimal VATAmount { get; set; }// need to make it nullable
        public int ExpenseAccountId { get; set; }// need to make it nullable
        public int VatAccountId { get; set; }// need to make it nullable



        // Scenario B: Supplier / Liability Settlement
        // Scenario C: Withholding // i think will represent supplier on this scenario
        public int? PayableAccountId { get; set; } // ده الحساب الي هيبا عليا ليه فلوس                                                    // 🔹 NEW (Used in Withholding)
        public decimal? GrossAmount { get; set; }   // إجمالي مستحق المورد قبل الخصم


    }
    public enum SettlementType
    {
        DirectExpense = 1,      // دفع مصروف مباشر (مع VAT)
        PayableSettlement = 2,  // سداد مورد / التزام سابق
        AdvancePayment = 3,     // دفعة مقدمة (Prepaid)
        TaxPayment = 4,         // سداد ضريبة مستحقة
        Refund = 5              // استرداد مبلغ (مرتجع)
    }
    public enum PaymentMethod 
    {
        Bank,
        Cash, 
    }
}

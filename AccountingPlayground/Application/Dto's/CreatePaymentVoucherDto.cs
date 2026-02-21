namespace AccountingPlayground.Application.Dto_s
{
    // are PayableAccountId will receive it from user or will be parent account and will get her "الرصيد" from journal entry  
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


        // Scenario D: Overpayment
        public int? CashAccountIdOverpayment { get; set; }
        public int? BankAccountIdOverpayment { get; set; }
        public long? PaidAmountOverpayment { get; set; }
        public int? PayableAccountIdOverpayment { get; set; } // this is supplier will return to him only money on Liability

        // this represent account id for additional money for me will save it on supplier
        // , to next time when by from him used this money 
        // for each supplier will have individual Supplier Advances her individual account id as leaf , not all account on same leaf
        public int? SupplierAdvancesIdOverpayment { get; set; }    // represent on asset 


        // Scenario E: Advance Payment  


        // Scenario F: Tax Payment  
        public int? VoucherId { get; set; } 


    }
    public enum SettlementType
    {
        DirectExpense = 1,      // دفع مصروف مباشر (مع VAT)
        PayableSettlement = 2,  // سداد مورد / التزام سابق
        AdvancePayment = 3,     // دفعة مقدمة (Prepaid)
        TaxPayment = 4,         // سداد ضريبة مستحقة
        Refund = 5,              // استرداد مبلغ (مرتجع)
        Overpayment,
    }
    public enum PaymentMethod 
    {
        Bank,
        Cash, 
    }
}

namespace AccountingPlayground.Domain.AccountingEntities.Enums
{
	public enum PaymentMethod
	{ 
		Cash = 1,
		Bank = 2
	}

	public enum ReceiptReferenceType
	{
		Customer = 1,
		Other = 2
	}
	public enum PaymentReferenceType
	{
		Supplier = 1,
		Tax = 2,
		Expense = 3
	}
}

using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Domain.Entities;

namespace AccountingPlayground.Domain.AccountingEntities
{
	public class Payment
	{
		public int Id { get; set; }
		public DateTime Date { get; set; }

		public long Amount { get; set; }          // بالقرش
		public PaymentType Type { get; set; }     // Pay / Receive

		public PaymentFor PaymentFor { get; set; }
		public int ReferenceId { get; set; }
	}

	public enum PaymentType
	{
		Pay = 1,       // سداد
		Receive = 2    // تحصيل
	}
	public enum PaymentFor
	{
		Supplier = 1,
		Customer = 2,
		Vat = 3,
		Expense = 4
	}
}
//Payment بيجاوب على:

//إمتى الدفع حصل؟

//كام اتدفع؟

//كان تحصيل ولا سداد؟

//كاش ولا بنك؟ (ممكن تضيفها بعدين)


//يعني:

//سجل تاريخي لحركة فلوس حقيقية

//من غيره:

//مش هتعرف:

//دفعت امتى

//دفعت قد إيه

//دفعت على إيه
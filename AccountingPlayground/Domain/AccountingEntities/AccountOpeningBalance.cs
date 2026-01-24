using AccountingPlayground.Domain.AccountingEntities.Enums;
using AccountingPlayground.Domain.Entities;

namespace AccountingPlayground.Domain.AccountingEntities
{
	public class AccountOpeningBalance
	{ 
		public int Id { get; set; }

		public int FinancialAccountId { get; set; }
		public FinancialAccount FinancialAccount { get; set; }

        public int FinancialYearId { get; set; }
        public FinancialYear FinancialYear { get; set; }

		public long OpeningDebit { get; set; }
		public long OpeningCredit { get; set; }
	}
}
// one FinancialAccountId with year


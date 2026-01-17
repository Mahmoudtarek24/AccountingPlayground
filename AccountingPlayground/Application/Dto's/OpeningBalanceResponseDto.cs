using AccountingPlayground.Domain.AccountingEntities;

namespace AccountingPlayground.Application.Dto_s
{
    public class OpeningBalanceResponseDto
    {
        public int FinancialYear { get; set; }

        public List<OpeningBalanceAccountDto> Accounts { get; set; } = new();
    }
    public class OpeningBalanceAccountDto
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; } 
        public AccountType Type { get; set; } 
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

}

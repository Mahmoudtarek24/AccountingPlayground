namespace AccountingPlayground.Application.Dto_s
{
    public class CreateOpeningBalanceDto
    {
        public List<OpeningBalanceItemDto> Items { get; set; } = new();
    }
    public class OpeningBalanceItemDto
    {
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
    }
}

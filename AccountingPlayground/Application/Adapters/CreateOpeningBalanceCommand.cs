namespace AccountingPlayground.Application.Adapters
{
    public class CreateOpeningBalanceCommand
    {
        public IReadOnlyList<OpeningBalanceItemCommand> Items { get; }
        public CreateOpeningBalanceCommand(IReadOnlyList<OpeningBalanceItemCommand> items)
        {
            Items = items;
        }
    }
    public class OpeningBalanceItemCommand
    {
        public int AccountId { get; }       
        public long Amount { get; }
        public OpeningBalanceItemCommand(int accountId, long amountInCents)
        {
            AccountId = accountId;
            Amount = amountInCents;
        }
    }
}

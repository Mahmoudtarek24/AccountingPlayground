using AccountingPlayground.Domain.AccountingEntities;

namespace AccountingPlayground.Domain.Interfaces
{
    public interface IJournalEntryRepository
    {
        Task<List<JournalEntryLine>> GetJournalEntryLinesOfAccount(int accountId);
    }
}

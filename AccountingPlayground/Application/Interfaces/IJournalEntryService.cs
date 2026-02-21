using AccountingPlayground.Application.Dto_s;

namespace AccountingPlayground.Application.Interfaces
{
    public interface IJournalEntryService
    {
        Task<JournalEntryError> PostJournalEntry(JournalEntryPostModel request);

        Task<bool> ReverseJournalEntry(int entryId , ReversalOptions  options = null);  

        // LockPeriod   قفل شهور   قفل أيام
    }
}

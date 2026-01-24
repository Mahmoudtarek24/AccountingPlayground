using AccountingPlayground.Application.Dto_s;

namespace AccountingPlayground.Application.Interfaces
{
    public interface IJournalEntryService
    {
        Task<JournalEntryError> PostJournalEntry(JournalEntryPostModel request);

        //ReverseJournalEntry  Replace Delete  قيد عكسي

        // LockPeriod   قفل شهور   قفل أيام
    }
}

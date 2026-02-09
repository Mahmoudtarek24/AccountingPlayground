using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Domain.Interfaces;
using AccountingPlayground.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AccountingPlayground.Infrastructure.Implementation
{
    public class JournalEntryRepository : IJournalEntryRepository
    {
        private readonly ApplicationDbContext context;
        public JournalEntryRepository(ApplicationDbContext context)
        {
            this.context = context; 
        }

        public async Task<List<JournalEntryLine>> GetJournalEntryLinesOfAccount(int accountId)
        {
            var result = await context.JournalEntryLines.AsNoTracking().
                      Where(e => e.FinancialAccountId == accountId && !e.JournalEntry.FinancialYear.IsClosed)
                                         .ToListAsync();

            // for more inhance should return only debit and credit ,

            return result;
        }
    }
}

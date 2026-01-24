using AccountingPlayground.Application.Dto_s;
using AccountingPlayground.Application.Interfaces;
using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Domain.Interfaces;
using AccountingPlayground.Infrastructure.Context;
using AccountingPlayground.Infrastructure.Implementation;
using Azure.Core;
using System.Threading.Tasks.Dataflow;

namespace AccountingPlayground.Application.Implementation
{
    public class JournalEntryService : IJournalEntryService
    {
        private readonly IJournalEntryRepository journalEntryRepository;
        private readonly IFinancialAccountRepository financialAccountRepository;
        private readonly IFinancialYearRepository financialYearRepository;
        private readonly ApplicationDbContext context;
        public JournalEntryService(IJournalEntryRepository journalEntryRepository
                                  ,IFinancialAccountRepository financialAccountRepository
                                  ,IFinancialYearRepository financialYearRepository
                                  ,ApplicationDbContext context)
        {
            this.journalEntryRepository = journalEntryRepository;
            this.financialAccountRepository = financialAccountRepository;   
            this.financialYearRepository = financialYearRepository;
            this.context= context;  
        }

        //ValidateEntryStructure
        //ValidateEntryBalance
        //ValidateAccounts
        //ValidateEntryDate
        //EnsurePeriodIsOpen
        //EnsureAccountsArePostable

        public async Task<JournalEntryError> PostJournalEntry(JournalEntryPostModel request)
        {
            if (!IsEntryStructureValid(request))
                return JournalEntryError.InvalidEntryStructure;

            var totalDebit = request.Lines.Sum(l => l.Debit);
            var totalCredit = request.Lines.Sum(l => l.Credit);
          
            if (totalDebit != totalCredit)
                return JournalEntryError.UnbalancedEntry;

            var accountIds = request.Lines.Select(l => l.FinancialAccountId).Distinct().ToList();

            var validAccountIds =  await financialAccountRepository.GetValidAccountIdsAsync(accountIds);

            if(validAccountIds.Count() != accountIds.Count())
                return JournalEntryError.UnbalancedEntry;

            if (!await financialYearRepository.IsPostingAllowedAsync(request.EntryDate))
                return JournalEntryError.ClosedPeriod;

            var journalEntry = new JournalEntry
            {
                Date = request.EntryDate,
                Reference = request.Reference,
               // FinancialYearId = request.FinancialYearId
            };

            foreach (var line in request.Lines)
                journalEntry.Lines.Add(new JournalEntryLine
                {
                    FinancialAccountId = line.FinancialAccountId,
                    Debit = line.Debit,
                    Credit = line.Credit
                });

            await context.JournalEntries.AddAsync(journalEntry);
            await context.SaveChangesAsync();

            return JournalEntryError.CreatedSuccessfully;

        }
        private bool IsEntryStructureValid(JournalEntryPostModel request)
        {
            if (request.Lines.Count < 2)
                return false;

            return request.Lines.All(IsLineStructureValid);
        }

        private bool IsLineStructureValid(JournalEntryLinePostModel line)
        {
            if (line.FinancialAccountId <= 0)
                return false;

            if (line.Debit < 0 || line.Credit < 0)
                return false;

            if (line.Debit == 0 && line.Credit == 0)
                return false;

            if (line.Debit > 0 && line.Credit > 0)
                return false;

            return true;
        }
    }
}
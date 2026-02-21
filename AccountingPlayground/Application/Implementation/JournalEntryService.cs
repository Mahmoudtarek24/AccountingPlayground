using AccountingPlayground.Application.Dto_s;
using AccountingPlayground.Application.Interfaces;
using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Domain.Interfaces;
using AccountingPlayground.Infrastructure.Context;
using AccountingPlayground.Infrastructure.Implementation;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
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

        public async Task<bool> ReverseJournalEntry(int entryId, ReversalOptions? options = null)
        {
            using var transaction = await context.Database.BeginTransactionAsync();

            var entry = await context.JournalEntries
                .Include(e => e.Lines)
                .SingleOrDefaultAsync(e => e.Id == entryId);

            if (entry == null)
                return false;

            if (entry.IsReversal)
                return false;

            decimal originalTotal = entry.Lines
                .Sum(l => l.Debit > 0 ? l.Debit : l.Credit);

            decimal ratio = 1m;

            if (options?.PartialPayment != null)
            {
                if (options.PartialPayment <= 0 || options.PartialPayment > originalTotal)
                    return false;

                ratio = options.PartialPayment.Value / originalTotal;
            }

            var reverse = new JournalEntry
            {
                Date = options?.ReversalDate ?? DateTime.UtcNow,
                Reference = $"Reversal of Entry #{entry.Id}",
                IsReversal = true,
                OriginalEntryId = entry.Id,
                Lines = new List<JournalEntryLine>()
            };

            var calculatedLines = new List<(int AccountId, decimal Debit, decimal Credit)>();

            foreach (var line in entry.Lines)
            {
                decimal originalAmount = line.Debit > 0 ? line.Debit : line.Credit;
                decimal reversedAmount = Math.Round(originalAmount * ratio, 2);

                if (line.Debit > 0)
                {
                    calculatedLines.Add((
                        line.FinancialAccountId,
                        0m,
                        reversedAmount
                    ));
                }
                else
                {
                    calculatedLines.Add((
                        line.FinancialAccountId,
                        reversedAmount,
                        0m
                    ));
                }
            }

            // 🔒 Ensure Perfect Balance After Rounding
            decimal debitSum = calculatedLines.Sum(l => l.Debit);
            decimal creditSum = calculatedLines.Sum(l => l.Credit);

            decimal difference = debitSum - creditSum;

            if (difference != 0)
            {
                // Adjust last line to absorb rounding difference
                var last = calculatedLines.Last();

                if (difference > 0)
                    last.Credit += difference;
                else
                    last.Debit += Math.Abs(difference);

                calculatedLines[calculatedLines.Count - 1] = last;
            }

            // Final Safety Check
            debitSum = calculatedLines.Sum(l => l.Debit);
            creditSum = calculatedLines.Sum(l => l.Credit);

            if (debitSum != creditSum)
                return false;

            // Convert to long (or decimal depending on your schema)
            foreach (var line in calculatedLines)
            {
                reverse.Lines.Add(new JournalEntryLine
                {
                    FinancialAccountId = line.AccountId,
                    Debit = (long)line.Debit,
                    Credit = (long)line.Credit
                });
            }

            entry.IsReversal = true;

            context.JournalEntries.Add(reverse);
            await context.SaveChangesAsync();

            return true;
        }
    }
}
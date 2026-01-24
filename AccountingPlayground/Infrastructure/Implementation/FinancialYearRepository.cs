using AccountingPlayground.Domain.Interfaces;
using AccountingPlayground.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AccountingPlayground.Infrastructure.Implementation
{
    public class FinancialYearRepository : IFinancialYearRepository
    {
        private readonly ApplicationDbContext context;
        public FinancialYearRepository(ApplicationDbContext context)
        {
            this.context = context; 
        }
        public async Task<bool> IsOpenAsync(int year)
            =>  await context.FinancialYear
                   .AnyAsync(fy => fy.Year == year && !fy.IsClosed);

        public async Task<bool> IsPostingAllowedAsync(DateTime entryDate)
            => await context.FinancialYear.AnyAsync(year =>
                !year.IsClosed &&
                entryDate.Date >= year.StartDate.Date &&
                entryDate.Date <= year.EndDate.Date
            );
    }
}

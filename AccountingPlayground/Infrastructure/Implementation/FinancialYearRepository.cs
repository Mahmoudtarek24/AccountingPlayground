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
        public async Task<bool> IsClosedAsync(int year)
            =>      await context.FinancialYear
                      .AnyAsync(fy => fy.Year == year && !fy.IsClosed);
    }
}

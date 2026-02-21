using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Domain.Interfaces;
using AccountingPlayground.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AccountingPlayground.Infrastructure.Implementation
{
    public class FinancialSettingsRepository : IFinancialSettingsRepository
    {
        private readonly ApplicationDbContext context;
        public FinancialSettingsRepository(ApplicationDbContext context)
        {
            this.context = context; 
        }
        public async Task<decimal?> GetServicesFee()
        {
            var result = await context.FinancialSettings.FirstOrDefaultAsync();
            
            return result?.ServiceFee;
        }

        public async Task<FinancialSettings?> GetSettings() =>
            await context.FinancialSettings.FirstOrDefaultAsync();    
    }

    public class TaxSettingRepository : ITaxSettingRepository
    {
        private readonly ApplicationDbContext context;
        public TaxSettingRepository(ApplicationDbContext context)
        {
            this.context = context;
        }
        public async Task<string?> GetValueAsync(string key)
            => await context.TaxSettings.Where(e=>e.Key == key)
                   .Select(e=>e.Value).FirstOrDefaultAsync();
    }
}

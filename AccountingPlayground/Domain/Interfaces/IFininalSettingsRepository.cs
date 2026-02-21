using AccountingPlayground.Domain.AccountingEntities;

namespace AccountingPlayground.Domain.Interfaces
{
    public interface IFinancialSettingsRepository
    {
        //Task<decimal?> GetServicesFee();
        Task<FinancialSettings?> GetSettings();
    }
    public interface ITaxSettingRepository
    { 
        Task<string?> GetValueAsync(string key);        
    } 
}


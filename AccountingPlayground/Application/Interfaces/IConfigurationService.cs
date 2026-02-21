using AccountingPlayground.Domain.AccountingEntities;

namespace AccountingPlayground.Application.Interfaces
{
    public interface IConfigurationService
    {
        //first approach
        // Task<decimal> GetServiceFee();


        // second approach
        Task<decimal> GetServiceFee();

        // third approach , key vale 
        Task<decimal> GetDecimalAsync(string key);
        Task<int> GetIntAsync(string key);
        Task<FinancialSettings> GetAllSetting();
    }
}

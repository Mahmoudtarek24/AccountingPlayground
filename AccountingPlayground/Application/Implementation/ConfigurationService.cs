using AccountingPlayground.Application.Interfaces;
using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace AccountingPlayground.Application.Implementation
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IMemoryCache cache;
        private readonly IFinancialSettingsRepository financialSettingsRepository;
        private readonly ITaxSettingRepository taxSettingRepository;
        public ConfigurationService(IMemoryCache cache, IFinancialSettingsRepository financialSettings, ITaxSettingRepository taxSettingRepository)
        {
            this.financialSettingsRepository = financialSettings;
            this.cache = cache;     
            this.taxSettingRepository = taxSettingRepository;       
        }

        // first approach
        //public async Task<decimal> GetServiceFee()
        //{
        //    const string cacheKey = "ServicesFee";

        //    if (!cache.TryGetValue(cacheKey, out decimal serviceFee))
        //    {
        //        var settings = await fininalSettingsRepository.GetSettings();

        //        if (settings is null)
        //            throw new InvalidOperationException("FininalSettings not configured.");

        //        cache.Set(cacheKey, settings.ServiceFee, TimeSpan.FromMinutes(30));
        //    }
        //    return serviceFee;
        //}



        // second approach  ,
        // on this style i will get table as cache after that i will return property to user as need like GetServiceFee
        public async Task<decimal> GetServiceFee()
        {
            var settings = await GetSettingsCached();
            return settings.ServiceFee;
        }
        private async Task<FinancialSettings> GetSettingsCached()
        {
            return await cache.GetOrCreateAsync("FininalSettings ", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                var settings = await financialSettingsRepository.GetSettings();
                if (settings is null)
                    throw new InvalidOperationException("FininalSettings not configured.");

                return settings;
            });
        }

        // thrid approach

        private async Task<T> GetValue<T>(string key, Func<string,T> converter)
        {
            var cacheKey = $"{key}";        
            if(!cache.TryGetValue(cacheKey, out string value))
            {
                value = await taxSettingRepository.GetValueAsync(key);

                if (value is null)
                    throw new InvalidOperationException(
                        $"Configuration key '{key}' not found.");

                cache.Set(cacheKey, value);
            }
            return converter(value);
        }
        public Task<decimal> GetDecimalAsync(string key)
            => GetValue(key, decimal.Parse);
        public  Task<int> GetIntAsync(string key)
            => GetValue(key, int.Parse);

        /// and this away used it on code 
        /// await _configurationService.GetIntAsync("Tax.Withholding");



        public async Task<FinancialSettings> GetAllSetting()
        {
            var cacheKey = "AllSetting";
            if(!cache.TryGetValue(cacheKey,out FinancialSettings value))
            {
                value = await financialSettingsRepository.GetSettings();

                if (value is null)
                    throw new Exception("FinancialSettings not configured.");
                cache.Set(cacheKey, value);        
            }
            return value ;
        }


    }
}

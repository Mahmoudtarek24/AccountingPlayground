using AccountingPlayground.Domain.AccountingEntities;

namespace AccountingPlayground.Domain.Interfaces
{
    public interface IFinancialYearRepository
    {
        Task<bool> IsOpenAsync(int year);
        //FinancialYear GetCurrentAsync();
        // FinancialYear GetByYear(int year);

        Task<bool> IsPostingAllowedAsync(DateTime entryDate);
    }
}

using AccountingPlayground.Domain.AccountingEntities;

namespace AccountingPlayground.Domain.Interfaces
{
    public interface IFinancialYearRepository
    {
        Task<bool> IsClosedAsync(int year);
        //FinancialYear GetCurrentAsync();
        // FinancialYear GetByYear(int year);
    }
}

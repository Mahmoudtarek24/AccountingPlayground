using AccountingPlayground.Domain.AccountingEntities;

namespace AccountingPlayground.Domain.Interfaces
{
    public interface IFinancialAccountRepository
    {
        Task<List<FinancialAccount>> GetChartOfAccountsTreeAsync();
        Task<FinancialAccount?> GetByIdAsync(int Id);
    }
}

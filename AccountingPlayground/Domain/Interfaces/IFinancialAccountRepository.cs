using AccountingPlayground.Domain.AccountingEntities;

namespace AccountingPlayground.Domain.Interfaces
{
    public interface IFinancialAccountRepository
    {
        Task<List<FinancialAccount>> GetChartOfAccountsTreeAsync();
        Task<FinancialAccount?> GetByIdAsync(int Id);
        Task<bool> IsValidName(int parentAccountId, string name);
        Task<List<int>> GetValidAccountIdsAsync(List<int> accountIds);
        Task<List<int>> GetValidAnyAccountTypeIdsAsync(List<int> accountIds);
    }
}

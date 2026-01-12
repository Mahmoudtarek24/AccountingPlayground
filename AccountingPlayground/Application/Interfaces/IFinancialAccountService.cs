using AccountingPlayground.Application.Dto_s;
using AccountingPlayground.Domain.AccountingEntities;

namespace AccountingPlayground.Application.Interfaces
{
    public interface IFinancialAccountService
    {
        Task<List<FinancialAccount>> GetChartOfAccountsTree();
        Task<int> CreateFinancialAccount(CreateFinancialAccountDto dto);
    }
}

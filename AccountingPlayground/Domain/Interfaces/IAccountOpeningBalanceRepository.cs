using AccountingPlayground.Domain.AccountingEntities;
using AccountingPlayground.Infrastructure.Context;

namespace AccountingPlayground.Domain.Interfaces
{
    public interface IAccountOpeningBalanceRepository
    {
        Task<IEnumerable<FinancialAccount>> GetEligibleAccountsForOpeningBalanceAsync();
        Task<List<int>> GetValidOpeningBalancesAsync(List<int> accountIds);

        //IEnumerable<OpeningBalanceDto> GetOpeningBalancesByYear(int year);

        //bool OpeningBalanceExists(int year);

        //void CreateOpeningBalance(int year, IEnumerable<OpeningBalanceItemDto> items);

        //OpeningBalanceStatus GetOpeningBalanceStatus(int year);

        //void LockOpeningBalance(int year);

        //void CarryForwardOpeningBalance(int fromYear, int toYear);

        //IEnumerable<OpeningBalanceAuditDto> GetOpeningBalanceAudit(int year);

    }
}

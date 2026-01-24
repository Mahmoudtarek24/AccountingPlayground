using AccountingPlayground.Application.Adapters;
using AccountingPlayground.Application.Dto_s;

namespace AccountingPlayground.Application.Interfaces
{
    public interface IAccountOpeningServices
    {
        Task<IEnumerable<AccountDto>> GetEligibleAccounts();
        Task<OpeningBalanceResponseDto> GetOpeningBalance(int year);
        Task<bool> CreateOpeningBalance(CreateOpeningBalanceCommand createDto);

        //OpeningBalanceValidationResult ValidateOpeningBalance(
        //    ValidateOpeningBalanceRequest request);

        //void LockOpeningBalance(int year);

        //OpeningBalanceStatus GetStatus(int year);

        Task CloseFinancialYear(int fromYear, int toYear);

        //IEnumerable<OpeningBalanceAuditDto> GetAuditLog(int year);

    }
}

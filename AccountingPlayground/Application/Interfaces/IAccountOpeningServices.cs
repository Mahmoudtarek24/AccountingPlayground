using AccountingPlayground.Application.Dto_s;

namespace AccountingPlayground.Application.Interfaces
{
    public interface IAccountOpeningServices
    {
        Task<IEnumerable<AccountDto>> GetEligibleAccounts();

        //IEnumerable<OpeningBalanceDto> GetOpeningBalance(int year);

        Task<bool> CreateOpeningBalance(CreateOpeningBalanceDto dto);

        //OpeningBalanceValidationResult ValidateOpeningBalance(
        //    ValidateOpeningBalanceRequest request);

        //void LockOpeningBalance(int year);

        //OpeningBalanceStatus GetStatus(int year);

        //void CarryForward(int fromYear, int toYear);

        //IEnumerable<OpeningBalanceAuditDto> GetAuditLog(int year);

    }
}

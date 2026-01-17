using AccountingPlayground.Application.Dto_s;
using AccountingPlayground.Application.Interfaces;
using AccountingPlayground.Domain.Interfaces;

namespace AccountingPlayground.Application.Implementation
{
    public class AccountOpeningServices : IAccountOpeningServices   
    {
        private readonly IFinancialAccountRepository financialAccountRepository;    
        private readonly IAccountOpeningBalanceRepository accountOpeningBalanceRepository;
        private readonly IFinancialYearRepository financialYearRepository;  

        public AccountOpeningServices(
            IFinancialAccountRepository financialAccountRepository,
            IAccountOpeningBalanceRepository accountOpeningBalanceRepository,
            IFinancialYearRepository financialYearRepository)
        {
            this.financialAccountRepository = financialAccountRepository;
            this.accountOpeningBalanceRepository = accountOpeningBalanceRepository;
            this.financialYearRepository = financialYearRepository;
        }

        public async Task<IEnumerable<AccountDto>> GetEligibleAccounts()
        {
            var result = await accountOpeningBalanceRepository.GetEligibleAccountsForOpeningBalanceAsync();

            return result.Select(e => new AccountDto
            {
                Id = e.Id,
                Code = e.Code,
                Name = e.Name,
                Type = e.Type
            }).ToList();
        }
    }
}

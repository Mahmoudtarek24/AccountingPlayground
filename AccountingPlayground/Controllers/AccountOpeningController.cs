using AccountingPlayground.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccountingPlayground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountOpeningController : ControllerBase
    {
        private readonly IAccountOpeningServices accountOpeningServices;

        public AccountOpeningController(IAccountOpeningServices accountOpeningServices)
        {
            this.accountOpeningServices = accountOpeningServices;
        }

        [HttpGet("eligible-accounts")]
        public async Task<IActionResult> GetEligibleAccounts()
        {
            var result = await accountOpeningServices.GetEligibleAccounts();        
            return Ok(result);  
        }
    }
}

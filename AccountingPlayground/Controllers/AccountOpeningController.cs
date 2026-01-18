using AccountingPlayground.Application.Adapters;
using AccountingPlayground.Application.Dto_s;
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
        private readonly CreateOpeningBalanceAdapter adapter;
        public AccountOpeningController(IAccountOpeningServices accountOpeningServices, CreateOpeningBalanceAdapter adapter)
        {
            this.accountOpeningServices = accountOpeningServices;
            this.adapter = adapter; 
        }

        [HttpGet("eligible-accounts")]
        public async Task<IActionResult> GetEligibleAccounts()
        {
            var result = await accountOpeningServices.GetEligibleAccounts();        
            return Ok(result);  
        }
        [HttpPost]
        public async Task<IActionResult> CreateOpeningBalance([FromBody] CreateOpeningBalanceDto dto)
        {
            var result = await adapter.Handle(dto);
            return Ok();
        }
    }
}

using AccountingPlayground.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccountingPlayground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinancialAccountController : ControllerBase
    {
        private readonly IFinancialAccountService financialService;
        public FinancialAccountController(IFinancialAccountService financialAccountService)
        {
            this.financialService = financialAccountService;
        }


        [HttpGet("allTree")]
        public async Task<IActionResult> Get()
        {
            var result = await financialService.GetChartOfAccountsTree();
            return Ok(result);
        }
    }
}

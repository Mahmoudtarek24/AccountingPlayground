using AccountingPlayground.Application.Dto_s;
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

        [HttpGet("tree")]
        public async Task<IActionResult> GetChartOfAccountsTree()
        {
            var result = await financialService.GetChartOfAccountsTree();
            return Ok(result);
        }

        [HttpGet("{id:int:min(1)}")]
        public async Task<IActionResult> GetById(int Id)
        {
            var result = await financialService.GetById(Id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFinancialAccountDto dto)
        {
            var result = await financialService.CreateFinancialAccount(dto);        
            return Ok(result);      
        }

    }
}

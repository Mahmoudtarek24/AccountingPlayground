using AccountingPlayground.Application.Dto_s;
using AccountingPlayground.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccountingPlayground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentVoucherController : ControllerBase
    {
        private readonly IPaymentVoucherService _service;

        public PaymentVoucherController(IPaymentVoucherService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePaymentVoucherDto dto)
        {
            var errors = await _service.CreatePaymentVoucher(dto);

            if (errors.Any())
                return BadRequest(new { errors });

            return Ok(new { message = "Payment voucher created successfully" });
        }

        [HttpPost("{id}/reverse")]
        public async Task<IActionResult> Reverse(int id, [FromQuery] long? partialPayment)
        {
            var result = await _service.ReversePaymentVoucher(id, partialPayment);

            if (!result)
                return BadRequest(new { message = "Failed to reverse payment voucher" });

            return Ok(new { message = "Payment voucher reversed successfully" });
        }
    }
}

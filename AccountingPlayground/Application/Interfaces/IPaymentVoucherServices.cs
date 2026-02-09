using AccountingPlayground.Application.Dto_s;

namespace AccountingPlayground.Application.Interfaces
{
	public interface IPaymentVoucherServices
	{
		Task<bool> CreatePaymentVoucher(CreatePaymentVoucherDto dto);

	}
}

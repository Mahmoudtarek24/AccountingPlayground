using AccountingPlayground.Application.Dto_s;

namespace AccountingPlayground.Application.Interfaces
{
	public interface IPaymentVoucherService
	{
        Task<List<string>> CreatePaymentVoucher(CreatePaymentVoucherDto dto);
        Task<bool> ReversePaymentVoucher(int voucherId, long? partialPayment = null);
    }
}

using AccountingPlayground.Application.Dto_s;

namespace AccountingPlayground.Application.Implementation.strategies__Pattern
{
    public interface IPaymentVoucherStrategy
    {
        SettlementType Type { get; }
        Task<List<string>> Validate(CreatePaymentVoucherDto dto);
        Task<List<string>> ExecuteAsync(CreatePaymentVoucherDto dto);
    }
}

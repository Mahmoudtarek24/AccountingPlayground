using AccountingPlayground.Application.Dto_s;

namespace AccountingPlayground.Application.Interfaces
{
    public interface IInvoiceServices
    {
        Task<bool> RegisterInvoice(CreateSupplierInvoice dto);
    }
}

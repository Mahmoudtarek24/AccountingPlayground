using AccountingPlayground.Domain.Entities;

namespace AccountingPlayground.Domain.Interfaces
{
    public interface ISupplierRepository
    {
        Task<bool> SupplierTaxRegistrationStatusAsync(string taxNumber);
        Task<Supplier?> GetSupplierById(int supplierId);
    }
}

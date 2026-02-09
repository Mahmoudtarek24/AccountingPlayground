using AccountingPlayground.Domain.Entities;
using AccountingPlayground.Domain.Interfaces;
using AccountingPlayground.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AccountingPlayground.Infrastructure.Implementation
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly ApplicationDbContext context;
        public SupplierRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Supplier?> GetSupplierById(int id)
            => await context.Suppliers.FindAsync(id);   

        public async Task<bool> SupplierTaxRegistrationStatusAsync(string taxNumber)
        {
           var result = await context.Suppliers.FirstOrDefaultAsync(e=>e.TaxNumber == taxNumber);

            return result is null ? false : result.TaxNumber is not null ? true : false;
        }
    }
}

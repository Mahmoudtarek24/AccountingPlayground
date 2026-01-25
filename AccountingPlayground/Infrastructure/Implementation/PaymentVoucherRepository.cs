using AccountingPlayground.Domain.Interfaces;
using AccountingPlayground.Infrastructure.Context;

namespace AccountingPlayground.Infrastructure.Implementation
{
	public class PaymentVoucherRepository : IPaymentVoucherRepository
	{
		private readonly ApplicationDbContext context;
		public PaymentVoucherRepository(ApplicationDbContext context)
		{
			this.context = context;	
		}
	}
}

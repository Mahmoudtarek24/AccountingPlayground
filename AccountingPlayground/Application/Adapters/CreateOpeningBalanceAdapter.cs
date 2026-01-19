using AccountingPlayground.Application.Dto_s;
using AccountingPlayground.Application.Interfaces;

namespace AccountingPlayground.Application.Adapters
{
    public class CreateOpeningBalanceAdapter
    {
		private readonly IAccountOpeningServices openingServices;

		public CreateOpeningBalanceAdapter(IAccountOpeningServices service)
		{
			this.openingServices = service;
		}

		public async Task<bool> Handle(CreateOpeningBalanceDto request)
		{
			var commandItems = request.Items
							  .Select(e =>
								  new OpeningBalanceItemCommand(
									  e.AccountId,
									  (long)(e.Amount * 100)
								  )).ToList();

			var command = new CreateOpeningBalanceCommand(commandItems);
			
			var result =await openingServices.CreateOpeningBalance(command);

			return result;
		}
	}
}

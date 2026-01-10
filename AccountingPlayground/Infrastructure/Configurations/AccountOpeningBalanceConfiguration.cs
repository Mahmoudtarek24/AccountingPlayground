using AccountingPlayground.Domain.AccountingEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountingPlayground.Infrastructure.Configurations
{
	public class AccountOpeningBalanceConfiguration : IEntityTypeConfiguration<AccountOpeningBalance>
	{
		void IEntityTypeConfiguration<AccountOpeningBalance>.Configure(EntityTypeBuilder<AccountOpeningBalance> builder)
		{
			builder.ToTable($"Fin_{nameof(AccountOpeningBalance)}");

			builder.HasOne(e => e.FinancialAccount)
				.WithMany(e => e.OpeningBalance).HasForeignKey(e => e.FinancialAccountId)
				.OnDelete(DeleteBehavior.Restrict);



			builder.HasCheckConstraint("CK_AccountOpeningBalance_DebitOrCredit"
				, "([OpeningDebit] > 0 AND [OpeningCredit] = 0) OR ([OpeningCredit] > 0 AND [OpeningDebit] = 0)");

		}
	}
}

using AccountingPlayground.Domain.AccountingEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountingPlayground.Infrastructure.Configurations
{
	public class FinancialAccountConfiguration : IEntityTypeConfiguration<FinancialAccount>
	{
		public void Configure(EntityTypeBuilder<FinancialAccount> builder)
		{
			builder.ToTable("Fin_FinancialAccounts");

			builder.HasKey(e => e.Id);
			builder.Property(x => x.Type).IsRequired();


			builder.HasMany(e=>e.Children).WithOne(e=>e.ParentAccount)
				.HasForeignKey(e=>e.ParentAccountId).OnDelete(DeleteBehavior.Restrict);
			builder.Property(e => e.Name).HasMaxLength(120);

		}
	}
}

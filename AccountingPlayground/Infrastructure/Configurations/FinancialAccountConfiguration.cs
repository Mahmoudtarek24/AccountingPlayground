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
            builder.Property(x => x.Code).IsRequired().HasMaxLength(20);


            builder.HasMany(e=>e.Children).WithOne(e=>e.ParentAccount)
				.HasForeignKey(e=>e.ParentAccountId).OnDelete(DeleteBehavior.Restrict);
			builder.Property(e => e.Name).HasMaxLength(120);

			builder.Property(e => e.SystemRole).HasConversion<string>()
				.HasDefaultValue(SystemAccountType.None);
			builder.HasIndex(e => new { e.Type, e.Code }).IsUnique();
			builder.HasCheckConstraint("CK_NoSelfParent", "[ParentAccountId] is null or [Id]<>[ParentAccountId]");
		}
	}
}

using AccountingPlayground.Domain.AccountingEntities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace AccountingPlayground.Infrastructure.Configurations
{
	public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
	{
		public void Configure(EntityTypeBuilder<Expense> builder)
		{
			builder.ToTable("Fin_Expenses");

			builder.HasKey(e => e.Id);

			builder.Property(e => e.Date).IsRequired();

			builder.Property(e => e.Type).IsRequired();

			builder.HasCheckConstraint("CK_Expense_PositiveAmount","[Amount] > 0");

		}
	}
}

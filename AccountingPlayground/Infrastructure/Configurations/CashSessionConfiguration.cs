using AccountingPlayground.Domain.AccountingEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountingPlayground.Infrastructure.Configurations
{
	public class CashSessionConfiguration : IEntityTypeConfiguration<CashSession>
	{
		public void Configure(EntityTypeBuilder<CashSession> builder)
		{
			builder.ToTable("Fin_CashSessions");

			builder.HasKey(e => e.Id);

			builder.Property(e => e.OpenedAt).IsRequired();


			builder.HasOne(e => e.Employee)
				.WithMany()
				.HasForeignKey(e => e.EmployeeId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasCheckConstraint("CK_CashSession_OpeningCash","[OpeningCash] >= 0");

			builder.HasCheckConstraint("CK_CashSession_CloseRequiresCash","[ClosedAt] IS NULL OR [ClosingCash] IS NOT NULL");

			builder.HasCheckConstraint("CK_CashSession_DateOrder","[ClosedAt] IS NULL OR [ClosedAt] >= [OpenedAt]");
		}
	}
}

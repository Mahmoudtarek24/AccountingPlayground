using AccountingPlayground.Domain.AccountingEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountingPlayground.Infrastructure.Configurations
{
	public class PaymentVoucherConfiguration : IEntityTypeConfiguration<PaymentVoucher>
	{
		public void Configure(EntityTypeBuilder<PaymentVoucher> builder)
		{
			builder.ToTable($"Fin_{nameof(PaymentVoucher)}");

			builder.HasKey(e => e.Id);

			builder.Property(e => e.VoucherNo)
				.IsRequired().HasMaxLength(50);

			builder.HasIndex(e => e.VoucherNo).IsUnique();

			builder.Property(e => e.VoucherDate).IsRequired();

			builder.Property(e => e.PaymentMethod).IsRequired();

			builder.HasOne(e => e.Employee)
				.WithMany()
				.HasForeignKey(e => e.EmployeeId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.CashSession)
				.WithMany()
				.HasForeignKey(e => e.CashSessionId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasCheckConstraint("CK_PaymentVoucher_PositiveAmount", "[Amount] > 0" );
		}
	}
}

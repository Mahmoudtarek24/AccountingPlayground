using AccountingPlayground.Domain.AccountingEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountingPlayground.Infrastructure.Configurations
{
	public class ReceiptVoucherConfiguration : IEntityTypeConfiguration<ReceiptVoucher>
	{
		public void Configure(EntityTypeBuilder<ReceiptVoucher> builder)
		{
			builder.ToTable($"Fin_{nameof(ReceiptVoucher)}");
			builder.HasKey(e => e.Id);

			builder.Property(e => e.VoucherNo)
				.IsRequired().HasMaxLength(50);

			builder.HasIndex(e => e.VoucherNo).IsUnique();

			builder.Property(e => e.VoucherDate).IsRequired();

			builder.Property(e => e.PaymentMethod).IsRequired();

			builder.Property(e => e.ReferenceType).IsRequired();

			builder.HasOne(e => e.Employee)
				.WithMany()
				.HasForeignKey(e => e.EmployeeId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.CashSession)
				.WithMany()
				.HasForeignKey(e => e.CashSessionId)
				.OnDelete(DeleteBehavior.Restrict);

			// 💡 المبلغ لازم يكون أكبر من صفر
			builder.HasCheckConstraint("CK_ReceiptVoucher_PositiveAmount", "[Amount] > 0" );

		}
	}
}

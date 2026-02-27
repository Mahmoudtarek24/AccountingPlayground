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
            builder.Property(e=>e.IsReversed).HasDefaultValue(false);	
			builder.Property(e => e.VoucherDate).IsRequired();
			builder.Property(e => e.PaymentMethod).HasConversion<string>().IsRequired();

            builder.Property(e => e.Status)
				.HasConversion<string>()
				.HasDefaultValue(VoucherStatus.Draft)
				.IsRequired();

            builder.HasOne(e => e.Supplier)
			   .WithMany()
			   .HasForeignKey(e => e.SupplierId)
			   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Employee)
				.WithMany()
				.HasForeignKey(e => e.EmployeeId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.CashSession)
				.WithMany()
				.HasForeignKey(e => e.CashSessionId)
				.OnDelete(DeleteBehavior.Restrict);


            builder.HasOne(e=>e.JournalEntry).WithOne()
				.HasForeignKey<PaymentVoucher>(e=>e.JournalEntryId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.OriginalVoucher).WithOne()
				.HasForeignKey<PaymentVoucher>(e => e.OriginalVoucherId)
				.OnDelete(DeleteBehavior.Restrict);

            builder.HasCheckConstraint("CK_PaymentVoucher_PositiveAmount", "[TotalAmount] > 0");
            builder.HasCheckConstraint("CK_PaymentVoucher_PreventSelRelation", "[OriginalVoucherId] <> [Id]");

            builder.HasCheckConstraint("CK_PaymentVoucher_ReversalConsistency",
				"([IsReversed] = 0 AND [OriginalVoucherId] IS NULL) OR ([IsReversed] = 1 AND [OriginalVoucherId] IS NOT NULL)");
        }
    }
}

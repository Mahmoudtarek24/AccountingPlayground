using AccountingPlayground.Domain.AccountingEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountingPlayground.Infrastructure.Configurations
{
    public class PaymentVoucherLineConfiguration : IEntityTypeConfiguration<PaymentVoucherLine>
    {
        public void Configure(EntityTypeBuilder<PaymentVoucherLine> builder)
        {
            builder.HasOne(e => e.PaymentVoucher)
                   .WithMany(e => e.Lines)
                   .HasForeignKey(e => e.PaymentVoucherId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.PurchaseInvoice)                
                .WithMany()
                .HasForeignKey(e => e.PurchaseInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasCheckConstraint("CK_PaymentVoucherLine_PositiveAmounts",
                "[Amount] >= 0 AND [TotalAmount] >= 0 AND ([VatAmount] IS NULL OR [VatAmount] >= 0)");

            // التأكد أن Total = Amount + VAT
            builder.HasCheckConstraint("CK_PaymentVoucherLine_TotalConsistency",
                "[TotalAmount] = [Amount] + ISNULL([VatAmount], 0)");
        }
    }
}

using AccountingPlayground.Domain.AccountingEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountingPlayground.Infrastructure.Configurations
{
	public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
	{
		public void Configure(EntityTypeBuilder<JournalEntry> builder)
		{
			builder.ToTable($"Fin_{nameof(JournalEntry)}");

			builder.HasKey( x => x.Id );
			builder.Property(e => e.Reference).IsRequired().HasMaxLength(130);
			builder.Property(e=>e.IsReversal).HasDefaultValue(false);		
            builder.Property(e=>e.OriginalEntryId).IsRequired(false);		

            builder.HasOne(e=>e.OriginalEntry)
				.WithOne(e=>e.ReversalEntry)
				.HasForeignKey<JournalEntry>(e=>e.OriginalEntryId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasCheckConstraint("CK_JournalEntry_NoSelfReverse", "[Id]<>[ReversedEntryId]");

        }
	}
}

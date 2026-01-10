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


		}
	}
}

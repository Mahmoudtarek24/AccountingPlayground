using AccountingPlayground.Domain.AccountingEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountingPlayground.Infrastructure.Configurations
{
	public class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
	{
		public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
		{
			builder.ToTable($"Fin_{nameof(JournalEntryLine)}");

			builder.HasKey(e => e.Id);
			builder.HasOne(e=>e.JournalEntry).WithMany(e=>e.Lines)
				.HasForeignKey(e=>e.JournalEntryId).OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.FinancialAccount).WithMany()
			.HasForeignKey(e => e.FinancialAccountId).OnDelete(DeleteBehavior.Restrict);

			builder.HasCheckConstraint("CK_JournalEntryLine_DebitOrCredit"
				, "([Debit] > 0 AND [Credit] = 0) OR ([Credit] > 0 AND [Debit] = 0)");
		}
	}
}

using AdSPMdS.DemanioDigitale.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdSPMdS.DemanioDigitale.Persistence.Configurations;

public class EmailOutboxMessageConfiguration : IEntityTypeConfiguration<EmailOutboxMessage>
{
    public void Configure(EntityTypeBuilder<EmailOutboxMessage> builder)
    {
        builder.ToTable("EmailOutboxMessages", "messaging");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.To).IsRequired().HasMaxLength(320);
        builder.Property(m => m.Subject).IsRequired().HasMaxLength(256);
        builder.Property(m => m.Body).IsRequired();
        builder.Property(m => m.HtmlBody);
        builder.Property(m => m.Template).HasMaxLength(128);
        builder.Property(m => m.PayloadJson);
        builder.Property(m => m.CcJson);
        builder.Property(m => m.BccJson);
        builder.Property(m => m.ReplyTo).HasMaxLength(320);
        builder.Property(m => m.AttachmentsJson);
        builder.Property(m => m.Status).IsRequired().HasMaxLength(32);
        builder.Property(m => m.LastError).HasMaxLength(1024);
        builder.Property(m => m.CreatedAtUtc).IsRequired();
        builder.HasIndex(m => new { m.Status, m.NextAttemptAtUtc });
    }
}

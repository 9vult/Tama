// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tama.Domain.Entities;

namespace Tama.Core.EntityConfig;

public sealed class BracketMessageConfiguration : IEntityTypeConfiguration<BracketMessage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<BracketMessage> msg)
    {
        msg.HasIndex(p => new { p.MessageId, p.BracketId }).IsUnique();
        msg.HasOne(p => p.Bracket)
            .WithMany()
            .HasForeignKey(p => p.BracketId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

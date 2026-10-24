// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tama.Domain.Entities;

namespace Tama.Core.EntityConfig;

public sealed class PollConfiguration : IEntityTypeConfiguration<Poll>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Poll> poll)
    {
        poll.HasIndex(p => new { p.MessageId, p.MatchId }).IsUnique();
        poll.HasOne(p => p.Match)
            .WithMany()
            .HasForeignKey(p => p.MatchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

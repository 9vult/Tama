// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tama.Domain.Entities;

namespace Tama.Core.EntityConfig;

public sealed class BracketConfiguration : IEntityTypeConfiguration<Bracket>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Bracket> bracket)
    {
        bracket
            .HasMany(b => b.Entries)
            .WithOne(e => e.Bracket)
            .HasForeignKey(e => e.BracketId)
            .OnDelete(DeleteBehavior.Cascade);
        bracket
            .HasMany(b => b.Matches)
            .WithOne(m => m.Bracket)
            .HasForeignKey(m => m.BracketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

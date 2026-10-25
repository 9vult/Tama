// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tama.Domain.Entities;

namespace Tama.Core.EntityConfig;

public sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Match> match)
    {
        match
            .HasIndex(m => new
            {
                m.BracketId,
                m.Round,
                m.Index,
            })
            .IsUnique();
        match
            .HasOne(m => m.Left)
            .WithMany()
            .HasForeignKey(m => m.LeftId)
            .OnDelete(DeleteBehavior.Restrict);
        match
            .HasOne(m => m.Right)
            .WithMany()
            .HasForeignKey(m => m.RightId)
            .OnDelete(DeleteBehavior.Restrict);
        match
            .HasOne(m => m.Winner)
            .WithMany()
            .HasForeignKey(m => m.WinnerId)
            .OnDelete(DeleteBehavior.Restrict);
        match
            .HasOne(m => m.Next)
            .WithMany()
            .HasForeignKey(m => m.NextId)
            .OnDelete(DeleteBehavior.NoAction);

        match.Ignore(m => m.SeedsLeftOfNext);

        match.Navigation(m => m.Left).AutoInclude();
        match.Navigation(m => m.Right).AutoInclude();
        match.Navigation(m => m.Winner).AutoInclude();
        match.Navigation(m => m.Next).AutoInclude();
    }
}

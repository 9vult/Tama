// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Tama.Domain.ValueObjects;

namespace Tama.Domain.Entities;

public sealed class Match
{
    public MatchId Id { get; set; } = MatchId.FromNewGuid();

    public required BracketId BracketId { get; set; }
    public Bracket Bracket { get; set; } = null!;

    [MaxLength(Length.Title)]
    public required string Title { get; set; }

    public required EntryId LeftId { get; set; }
    public Entry Left { get; set; } = null!;

    public required EntryId RightId { get; set; }
    public Entry Right { get; set; } = null!;

    public bool IsComplete { get; set; }

    public required EntryId? WinnerId { get; set; }
    public Entry? Winner { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

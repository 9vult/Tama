// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Tama.Domain.Enums;
using Tama.Domain.ValueObjects;

namespace Tama.Domain.Entities;

public sealed class Bracket
{
    public BracketId Id { get; set; } = BracketId.FromNewGuid();

    public required GroupId GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public required UserId CreatorId { get; set; }
    public User Creator { get; set; } = null!;

    [MaxLength(Length.Title)]
    public required string Title { get; set; }

    public required MatchDuration MatchDuration { get; set; }

    public bool IsComplete { get; set; }

    public required EntryId? WinnerId { get; set; }
    public Entry? Winner { get; set; } = null!;

    public ICollection<Entry> Entries { get; set; } = [];
    public ICollection<Match> Matches { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

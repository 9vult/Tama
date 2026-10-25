// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Tama.Domain.Enums;
using Tama.Domain.ValueObjects;

namespace Tama.Domain.Entities;

public sealed class Bracket
{
    public BracketId Id { get; init; } = BracketId.FromNewGuid();

    public required GroupId GroupId { get; init; }
    public required ChannelId ChannelId { get; init; }
    public required UserId CreatorId { get; init; }

    /// <summary>
    /// Title of the bracket
    /// </summary>
    [MaxLength(Length.Title)]
    public required string Title { get; init; }

    /// <summary>
    /// <see langword="true"/> if ties are allowed in the finals
    /// </summary>
    public required bool AllowTie { get; init; }

    /// <summary>
    /// Number of entries to allow
    /// </summary>
    public required int EntryLimit { get; init; }

    /// <summary>
    /// Number of matches to run concurrently
    /// </summary>
    public required int MaxConcurrentMatches { get; init; }

    /// <summary>
    /// Duration of each match
    /// </summary>
    public required MatchDuration MatchDuration { get; init; }

    /// <summary>
    /// Allow opening theme submissions
    /// </summary>
    public required bool AllowOpenings { get; init; }

    /// <summary>
    /// Allow ending theme submissions
    /// </summary>
    public required bool AllowEndings { get; init; }

    /// <summary>
    /// Allow insert theme submissions
    /// </summary>
    public required bool AllowInserts { get; init; }

    /// <summary>
    /// Display the submitters for each match (Does not apply to champion announcements)
    /// </summary>
    public required bool IsAnonymous { get; init; }

    /// <summary>
    /// Current state of the bracket
    /// </summary>
    public MatchState State { get; set; } = MatchState.Pending;

    /// <summary>
    /// List of entries
    /// </summary>
    public ICollection<Entry> Entries { get; init; } = [];

    /// <summary>
    /// List of matches
    /// </summary>
    public ICollection<Match> Matches { get; set; } = [];

    /// <summary>
    /// Time the bracket was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

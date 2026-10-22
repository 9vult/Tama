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

    public required ChannelId ChannelId { get; set; }
    public Channel Channel { get; set; } = null!;

    public required UserId CreatorId { get; set; }
    public User Creator { get; set; } = null!;

    /// <summary>
    /// Title of the bracket
    /// </summary>
    [MaxLength(Length.Title)]
    public required string Title { get; set; }

    /// <summary>
    /// <see langword="true"/> if ties are allowed in the finals
    /// </summary>
    public required bool AllowTie { get; set; }

    /// <summary>
    /// Number of entries to allow
    /// </summary>
    public required int EntryLimit { get; set; }

    /// <summary>
    /// Number of matches to run concurrently
    /// </summary>
    public required int MaxConcurrentMatches { get; set; }

    /// <summary>
    /// Duration of each match
    /// </summary>
    public required MatchDuration MatchDuration { get; set; }

    /// <summary>
    /// Allow opening theme submissions
    /// </summary>
    public required bool AllowOpenings { get; set; }

    /// <summary>
    /// Allow ending theme submissions
    /// </summary>
    public required bool AllowEndings { get; set; }

    /// <summary>
    /// Allow insert theme submissions
    /// </summary>
    public required bool AllowInserts { get; set; }

    /// <summary>
    /// Current state of the bracket
    /// </summary>
    public MatchState State { get; set; } = MatchState.Pending;

    /// <summary>
    /// List of champion IDs
    /// </summary>
    public ICollection<EntryId> ChampionIds { get; set; } = [];

    /// <summary>
    /// List of champions
    /// </summary>
    public ICollection<Entry> Champions { get; set; } = [];

    /// <summary>
    /// List of entries
    /// </summary>
    public ICollection<Entry> Entries { get; set; } = [];

    /// <summary>
    /// List of matches
    /// </summary>
    public ICollection<Match> Matches { get; set; } = [];

    /// <summary>
    /// Time the bracket was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

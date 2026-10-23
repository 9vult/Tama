// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.Enums;
using Tama.Domain.ValueObjects;

namespace Tama.Domain.Entities;

public sealed class Match
{
    public MatchId Id { get; init; } = MatchId.FromNewGuid();

    public required BracketId BracketId { get; init; }
    public Bracket Bracket { get; init; } = null!;

    /// <summary>
    /// Round in the bracket, 0-indexed
    /// </summary>
    public required int Round { get; init; }

    /// <summary>
    /// Position within the round, 0-indexed
    /// </summary>
    public required int Index { get; init; }

    /// <summary>
    /// ID of the match this match seeds into. <see langword="null"/> if this is the finals
    /// </summary>
    public MatchId? NextId { get; set; }

    /// <summary>
    /// Match this match seeds into. <see langword="null"/> if this is the finals
    /// </summary>
    public Match? Next { get; set; }

    /// <summary>
    /// <see langword="true"/> if this is the final match
    /// </summary>
    public bool IsFinal => Next is null;

    /// <summary>
    /// Current state of the match
    /// </summary>
    public MatchState State { get; init; } = MatchState.Pending;

    /// <summary>
    /// ID of the entry in the left box. <see langword="null"/> if position is TBD.
    /// </summary>
    public EntryId? LeftId { get; set; }

    /// <summary>
    /// Entry in the left box. <see langword="null"/> if position is TBD.
    /// </summary>
    public Entry? Left { get; set; }

    /// <summary>
    /// ID of the entry in the right box. <see langword="null"/> if position is TBD.
    /// </summary>
    public EntryId? RightId { get; set; }

    /// <summary>
    /// Entry in the right box. <see langword="null"/> if position is TBD.
    /// </summary>
    public Entry? Right { get; set; }

    /// <summary>
    /// ID of the winning entry. <see langword="null"/> if no winner has been decided.
    /// </summary>
    public EntryId? WinnerId { get; set; }

    /// <summary>
    /// The winning entry. <see langword="null"/> if no winner has been decided.
    /// </summary>
    public Entry? Winner { get; set; }

    /// <summary>
    /// <see langword="true"/> if this match is ready to be started
    /// </summary>
    public bool IsReady => State is MatchState.Pending && Left is not null && Right is not null;

    /// <summary>
    /// Time the match was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

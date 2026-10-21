// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Tama.Domain.Entities;

namespace Tama.Domain.ValueObjects;

public sealed class Entry
{
    public EntryId Id { get; set; } = EntryId.FromNewGuid();

    public required BracketId BracketId { get; set; }
    public Bracket Bracket { get; set; } = null!;

    public required UserId SubmitterId { get; set; }
    public User Submitter { get; set; } = null!;

    public required int Seed { get; set; }

    [MaxLength(Length.Title)]
    public required string Title { get; set; }

    [MaxLength(Length.Url)]
    public required string Url { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

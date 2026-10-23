// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Tama.Domain.ValueObjects;

namespace Tama.Domain.Entities;

public sealed class Entry
{
    public EntryId Id { get; set; } = EntryId.FromNewGuid();

    public required BracketId BracketId { get; set; }
    public Bracket Bracket { get; set; } = null!;

    public required UserId SubmitterId { get; set; }

    public required int Seed { get; set; }

    [MaxLength(Length.Title)]
    public required string Title { get; set; }

    [MaxLength(Length.Url)]
    public required string ImageUrl { get; set; }

    [MaxLength(Length.Url)]
    public required string SubmissionUrl { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

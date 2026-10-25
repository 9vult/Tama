// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.ValueObjects;

namespace Tama.Domain.Entities;

public sealed class BracketMessage
{
    public int Id { get; set; }
    public required MessageId MessageId { get; init; }
    public required BracketId BracketId { get; init; }
    public Bracket Bracket { get; set; } = null!;
}

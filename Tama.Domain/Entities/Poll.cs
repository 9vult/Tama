// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.ValueObjects;

namespace Tama.Domain.Entities;

public sealed class Poll
{
    public int Id { get; set; }
    public required MessageId MessageId { get; init; }
    public required MatchId MatchId { get; init; }
    public Match Match { get; set; } = null!;
}

// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.ValueObjects;

namespace Tama.Core.Events;

public sealed record TiebreakRequestedEvent(
    MatchId MatchId,
    ChannelId ChannelId,
    MessageId MessageId
) : IEvent;

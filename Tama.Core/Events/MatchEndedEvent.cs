// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.Enums;
using Tama.Domain.ValueObjects;

namespace Tama.Core.Events;

public sealed record MatchEndedEvent(MatchId MatchId, PollResult Result) : IEvent;

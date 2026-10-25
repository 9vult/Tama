// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.ValueObjects;

namespace Tama.Core.Features.Queries.PollMessages.Get;

public sealed record GetPollMessageResponse(MessageId MessageId, MatchId MatchId);

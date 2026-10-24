// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.ValueObjects;

namespace Tama.Core.Features.Queries.Polls.Get;

public sealed record GetPollResponse(MessageId MessageId, MatchId MatchId);

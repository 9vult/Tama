// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.ValueObjects;

namespace Tama.Core.Features.Queries.Matches;

public sealed record GetMatchInfoQuery(MatchId MatchId) : IQuery;

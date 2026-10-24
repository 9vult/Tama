// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.ValueObjects;

namespace Tama.Core.Features.Queries.Match;

public sealed record GetNextMatchQuery(BracketId BracketId) : IQuery;

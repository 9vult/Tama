// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.ValueObjects;

namespace Tama.Core.Features.Queries.BracketMessages.Get;

public sealed record GetBracketMessageQuery(BracketId BracketId) : IQuery;

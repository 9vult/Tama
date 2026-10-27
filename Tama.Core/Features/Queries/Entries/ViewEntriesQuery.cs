// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.ValueObjects;

namespace Tama.Core.Features.Queries.Entries;

public sealed record ViewEntriesQuery(BracketId BracketId, int Page) : IQuery;

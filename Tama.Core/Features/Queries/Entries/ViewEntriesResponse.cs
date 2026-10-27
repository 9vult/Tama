// SPDX-License-Identifier: MPL-2.0

using Tama.Core.Dtos;

namespace Tama.Core.Features.Queries.Entries;

public sealed record ViewEntriesResponse(
    string BracketTitle,
    IReadOnlyList<EntryDto> Entries,
    int Page,
    int PageCount
);

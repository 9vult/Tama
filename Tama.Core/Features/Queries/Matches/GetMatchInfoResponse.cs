// SPDX-License-Identifier: MPL-2.0

using Tama.Core.Dtos;

namespace Tama.Core.Features.Queries.Matches;

public sealed record GetMatchInfoResponse(
    string BracketTitle,
    string MatchTitle,
    EntryDto? Left,
    EntryDto? Right
);

// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.Entities;
using Tama.Domain.Enums;
using Tama.Domain.ValueObjects;

namespace Tama.Core.Dtos;

public sealed record BracketDto(
    BracketId Id,
    ChannelId ChannelId,
    string Title,
    int EntryLimit,
    MatchDuration MatchDuration,
    bool AllowOpenings,
    bool AllowEndings,
    bool AllowInserts,
    bool IsAnonymous
)
{
    public static BracketDto FromBracket(Bracket bracket)
    {
        return new BracketDto(
            bracket.Id,
            bracket.ChannelId,
            bracket.Title,
            bracket.EntryLimit,
            bracket.MatchDuration,
            bracket.AllowOpenings,
            bracket.AllowEndings,
            bracket.AllowInserts,
            bracket.IsAnonymous
        );
    }
}

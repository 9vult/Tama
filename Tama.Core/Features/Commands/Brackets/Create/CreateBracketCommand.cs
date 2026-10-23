// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.Enums;
using Tama.Domain.ValueObjects;

namespace Tama.Core.Features.Commands.Brackets.Create;

public sealed record CreateBracketCommand(
    GroupId GroupId,
    ChannelId ChannelId,
    UserId RequestedBy,
    string Title,
    bool AllowTie,
    int EntryLimit,
    int MaxConcurrentMatches,
    MatchDuration MatchDuration,
    bool AllowOpenings,
    bool AllowEndings,
    bool AllowInserts
) : ICommand;

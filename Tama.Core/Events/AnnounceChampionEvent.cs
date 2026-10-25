// SPDX-License-Identifier: MPL-2.0

using Tama.Core.Dtos;
using Tama.Domain.ValueObjects;

namespace Tama.Core.Events;

public sealed record AnnounceChampionEvent(
    BracketId BracketId,
    string BracketTitle,
    ChannelId ChannelId,
    IList<EntryDto> Champions
) : IEvent;

// SPDX-License-Identifier: MPL-2.0

using Tama.Core.Dtos;

namespace Tama.Core.Events;

public sealed record BracketCreatedEvent(BracketDto BracketDto) : IEvent;

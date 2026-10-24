// SPDX-License-Identifier: MPL-2.0

using Tama.Core.Events;
using Tama.Domain.ValueObjects;

namespace Tama.Core.EventHandlers;

public sealed record BracketReadyEvent(BracketId BracketId) : IEvent;

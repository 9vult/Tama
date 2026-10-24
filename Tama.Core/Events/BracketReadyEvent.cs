// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.ValueObjects;

namespace Tama.Core.Events;

public sealed record BracketReadyEvent(BracketId BracketId) : IEvent;

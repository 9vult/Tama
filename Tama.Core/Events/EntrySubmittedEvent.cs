// SPDX-License-Identifier: MPL-2.0

using Tama.Core.Dtos;
using Tama.Domain.ValueObjects;

namespace Tama.Core.Events;

public sealed record EntrySubmittedEvent(BracketDto Bracket, int CurrentSubmissionCount) : IEvent;

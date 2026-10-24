// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.ValueObjects;

namespace Tama.Core.Features.Commands.Polls.Create;

public sealed record CreatePollCommand(MatchId MatchId, MessageId MessageId) : ICommand;

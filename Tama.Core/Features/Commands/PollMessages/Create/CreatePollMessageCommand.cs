// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.ValueObjects;

namespace Tama.Core.Features.Commands.PollMessages.Create;

public sealed record CreatePollMessageCommand(MatchId MatchId, MessageId MessageId) : ICommand;

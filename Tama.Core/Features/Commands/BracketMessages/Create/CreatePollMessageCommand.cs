// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.ValueObjects;

namespace Tama.Core.Features.Commands.BracketMessages.Create;

public sealed record CreateBracketMessageCommand(BracketId BracketId, MessageId MessageId)
    : ICommand;

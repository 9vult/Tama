// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.Entities;
using static Tama.Core.Features.Result;

namespace Tama.Core.Features.Commands.BracketMessages.Create;

public sealed class CreateBracketMessageHandler(TamaDbContext db)
    : ICommandHandler<CreateBracketMessageCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(CreateBracketMessageCommand command)
    {
        await db.BracketMessages.AddAsync(
            new BracketMessage { BracketId = command.BracketId, MessageId = command.MessageId }
        );
        await db.SaveChangesAsync();
        return Success();
    }
}

// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.Entities;
using static Tama.Core.Features.Result;

namespace Tama.Core.Features.Commands.PollMessages.Create;

public sealed class CreatePollMessageHandler(TamaDbContext db)
    : ICommandHandler<CreatePollMessageCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(CreatePollMessageCommand command)
    {
        await db.PollMessages.AddAsync(
            new PollMessage { MatchId = command.MatchId, MessageId = command.MessageId }
        );
        await db.SaveChangesAsync();
        return Success();
    }
}

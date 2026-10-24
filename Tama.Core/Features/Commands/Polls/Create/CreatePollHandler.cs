// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.Entities;
using static Tama.Core.Features.Result;

namespace Tama.Core.Features.Commands.Polls.Create;

public sealed class CreatePollHandler(TamaDbContext db) : ICommandHandler<CreatePollCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(CreatePollCommand command)
    {
        await db.Polls.AddAsync(
            new Poll { MatchId = command.MatchId, MessageId = command.MessageId }
        );
        await db.SaveChangesAsync();
        return Success();
    }
}

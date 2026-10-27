// SPDX-License-Identifier: MPL-2.0

using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tama.Core.Dtos;
using Tama.Core.Events;
using Tama.Domain.Entities;
using Tama.Domain.Enums;
using static Tama.Core.Features.Result<Tama.Core.Features.Commands.Brackets.Create.CreateBracketResponse>;

namespace Tama.Core.Features.Commands.Brackets.Create;

public class CreateBracketHandler(
    TamaDbContext db,
    IEventBus eventBus,
    ILogger<CreateBracketHandler> logger
) : ICommandHandler<CreateBracketCommand, Result<CreateBracketResponse>>
{
    /// <inheritdoc />
    public async Task<Result<CreateBracketResponse>> HandleAsync(CreateBracketCommand command)
    {
        if (command.EntryLimit < 2 || !BitOperations.IsPow2(command.EntryLimit))
            return Fail(ResultStatus.BadRequest, "EntryLimit is not a power of 2");

        var bracket = new Bracket
        {
            GroupId = command.GroupId,
            ChannelId = command.ChannelId,
            RoleId = command.RoleId,
            CreatorId = command.RequestedBy,
            Title = command.Title,
            Description = command.Description,
            AllowTie = command.AllowTie,
            EntryLimit = command.EntryLimit,
            MaxConcurrentMatches = command.MaxConcurrentMatches,
            MatchDuration = command.MatchDuration,
            AllowOpenings = command.AllowOpenings,
            AllowEndings = command.AllowEndings,
            AllowInserts = command.AllowInserts,
            IsAnonymous = command.IsAnonymous,
        };

        logger.LogInformation(
            "Creating bracket {Title} with {EntryLimit} entries",
            bracket.Title,
            bracket.EntryLimit
        );

        await db.Brackets.AddAsync(bracket);
        await db.SaveChangesAsync();

        await eventBus.PublishAsync(new BracketCreatedEvent(BracketDto.FromBracket(bracket)));

        return Success(new CreateBracketResponse(bracket.Id));
    }
}

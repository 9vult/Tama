// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tama.Core.Events;
using Tama.Core.Services;
using Tama.Domain.Entities;
using Tama.Domain.Enums;
using static Tama.Core.Features.Result;

namespace Tama.Core.Features.Commands.Entries.Submit;

public sealed class SubmitEntriesHandler(
    TamaDbContext db,
    IEventBus eventBus,
    IAnimeThemesService atService,
    ILogger<SubmitEntriesHandler> logger
) : ICommandHandler<SubmitEntriesCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(SubmitEntriesCommand command)
    {
        var bracket = await db
            .Brackets.Include(b => b.Entries)
            .FirstOrDefaultAsync(b => b.Id == command.BracketId);

        if (bracket is null)
        {
            logger.LogError("Bracket {BracketId} does not exist", command.BracketId);
            return Fail(ResultStatus.NoBracket);
        }

        if (bracket.State is not MatchState.Pending)
            return Fail(ResultStatus.BadRequest, "Bracket is in progress");

        // Check if the user already submitted
        if (bracket.Entries.Any(e => e.SubmitterId == command.SubmitterId))
        {
            logger.LogError(
                "Submitter {SubmitterId} already submitted to bracket {BracketId}",
                command.SubmitterId,
                bracket.Id
            );
            return Fail(ResultStatus.AlreadySubmitted);
        }

        // Ensure all submissions are valid AT URLs
        if (
            !atService.ValidateUrl(command.FirstSubmission)
            || !atService.ValidateUrl(command.SecondSubmission)
            || !atService.ValidateUrl(command.ThirdSubmission)
        )
            return Fail(ResultStatus.BadRequest, "URL");

        // Ensure all submissions are of allowed types
        if (
            !atService.ValidateType(
                command.FirstSubmission,
                bracket.AllowOpenings,
                bracket.AllowEndings,
                bracket.AllowInserts
            )
            || !atService.ValidateType(
                command.SecondSubmission,
                bracket.AllowOpenings,
                bracket.AllowEndings,
                bracket.AllowInserts
            )
            || !atService.ValidateType(
                command.ThirdSubmission,
                bracket.AllowOpenings,
                bracket.AllowEndings,
                bracket.AllowInserts
            )
        )
            return Fail(ResultStatus.BadRequest, "TYPE");

        // Find the first submission that hasn't been submitted yet
        Uri submission;
        var existingSubs = bracket.Entries.Select(e => e.UniqueThemeId).ToHashSet();
        if (!existingSubs.Contains(atService.GetUniqueThemeId(command.FirstSubmission)))
            submission = command.ThirdSubmission;
        else if (!existingSubs.Contains(atService.GetUniqueThemeId(command.SecondSubmission)))
            submission = command.SecondSubmission;
        else if (!existingSubs.Contains(atService.GetUniqueThemeId(command.ThirdSubmission)))
            submission = command.ThirdSubmission;
        else
            return Fail(ResultStatus.EntryConflict);

        var animeDetails = await atService.GetAnimeDetailsAsync(submission);

        if (animeDetails.Title == string.Empty)
            return Fail(ResultStatus.Error);

        var entry = new Entry
        {
            BracketId = bracket.Id,
            SubmitterId = command.SubmitterId,
            Title = $"{animeDetails.Title} - {atService.GetTheme(submission)}",
            UniqueThemeId = atService.GetUniqueThemeId(submission),
            ImageUrl = animeDetails.ImageUrl,
            SubmissionUrl = submission.AbsoluteUri,
        };

        logger.LogInformation(
            "Adding user {UserId}'s entry to bracket {BracketId}, {EntryId}",
            command.SubmitterId,
            bracket.Id,
            entry.Id
        );

        await db.Entries.AddAsync(entry);
        await db.SaveChangesAsync();

        await eventBus.PublishAsync(new EntrySubmittedEvent(bracket.Id));
        return Success();
    }
}

// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tama.Core.Dtos;
using Tama.Core.Events;
using Tama.Domain.Enums;

namespace Tama.Core.EventHandlers;

public sealed class MatchEndedEventHandler(
    TamaDbContext db,
    IEventBus eventBus,
    ILogger<MatchEndedEventHandler> logger
) : IEventHandler<MatchEndedEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(MatchEndedEvent @event)
    {
        var bracket = await db
            .Brackets.Include(b => b.Matches)
            .FirstOrDefaultAsync(b => b.Matches.Any(m => m.Id == @event.MatchId));

        if (bracket is null)
            return;

        bracket.LinkMatches();

        var match = bracket.Matches.First(m => m.Id == @event.MatchId);
        if (match.State is not MatchState.Active)
            return;

        match.State = MatchState.Complete;
        match.WinnerId = @event.Result is PollResult.Left ? match.LeftId : match.RightId;

        if (match.Next is null) // Final
        {
            bracket.State = MatchState.Complete;
            await eventBus.PublishAsync(new AnnounceChampionEvent([match.WinnerId!.Value]));
        }
        else
        {
            if (match.SeedsLeftOfNext)
                match.Next.LeftId = match.WinnerId;
            else
                match.Next.RightId = match.WinnerId;

            var freeMatchSlots =
                bracket.MaxConcurrentMatches
                - bracket.Matches.Count(m => m.State is MatchState.Active);

            var nextMatches = bracket
                .Matches.Where(m => m.IsReady)
                .OrderBy(m => m.Round)
                .ThenBy(m => m.Index)
                .Take(Math.Max(0, freeMatchSlots))
                .ToList();

            foreach (var nextMatch in nextMatches)
            {
                nextMatch.State = MatchState.Active;
                await eventBus.PublishAsync(
                    new MatchStartedEvent(
                        nextMatch.Id,
                        new EntryDto(
                            nextMatch.Left!.Id,
                            nextMatch.Left.SubmitterId,
                            nextMatch.Left.Title,
                            nextMatch.Left.ImageUrl,
                            nextMatch.Left.SubmissionUrl
                        ),
                        new EntryDto(
                            nextMatch.Right!.Id,
                            nextMatch.Right.SubmitterId,
                            nextMatch.Right.Title,
                            nextMatch.Right.ImageUrl,
                            nextMatch.Right.SubmissionUrl
                        ),
                        bracket.Title,
                        nextMatch.Title,
                        bracket.DisplaySubmitters,
                        bracket.ChannelId,
                        bracket.MatchDuration
                    )
                );
            }
        }

        await db.SaveChangesAsync();
    }
}

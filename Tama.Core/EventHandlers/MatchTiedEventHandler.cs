// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tama.Core.Dtos;
using Tama.Core.Events;
using Tama.Domain.Enums;

namespace Tama.Core.EventHandlers;

public sealed class MatchTiedEventHandler(
    TamaDbContext db,
    IEventBus eventBus,
    ILogger<EventHandler> logger
) : IEventHandler<MatchTiedEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(MatchTiedEvent @event)
    {
        var bracket = await db
            .Brackets.Include(b => b.Matches)
            .FirstOrDefaultAsync(b => b.Matches.Any(m => m.Id == @event.MatchId));

        if (bracket is null)
            return;

        var match = bracket.Matches.First(m => m.Id == @event.MatchId);
        if (match.State is not MatchState.Active)
            return;

        if (match.Next is null && bracket.AllowTie) // Final, co-champions
        {
            logger.LogInformation("Co-champions picked for bracket {BracketId}", bracket.Id);
            match.State = MatchState.Complete;
            bracket.State = MatchState.Complete;
            await eventBus.PublishAsync(
                new AnnounceChampionEvent([match.LeftId!.Value, match.RightId!.Value])
            );
        }
        else
        {
            logger.LogInformation("Requesting tiebreak for match {MatchId}", match.Id);
            match.State = MatchState.AwaitingTiebreak;

            // Request tiebreak
            await eventBus.PublishAsync(
                new TiebreakRequestedEvent(
                    match.Id,
                    new EntryDto(
                        match.Left!.Id,
                        match.Left.SubmitterId,
                        match.Left.Title,
                        match.Left.ImageUrl,
                        match.Left.SubmissionUrl
                    ),
                    new EntryDto(
                        match.Right!.Id,
                        match.Right.SubmitterId,
                        match.Right.Title,
                        match.Right.ImageUrl,
                        match.Right.SubmissionUrl
                    ),
                    bracket.Title,
                    match.Title,
                    bracket.ChannelId
                )
            );

            // Check if any matches can start
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

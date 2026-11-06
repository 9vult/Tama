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

        // Allow time for poll close messages to show up
        await Task.Delay(TimeSpan.FromSeconds(bracket.MaxConcurrentMatches * 4));

        if (match.NextId is null && bracket.AllowTie) // Final, co-champions
        {
            logger.LogInformation("Co-champions picked for bracket {BracketId}", bracket.Id);
            match.State = MatchState.Complete;
            bracket.State = MatchState.Complete;
            await eventBus.PublishAsync(
                new AnnounceChampionEvent(
                    bracket.Id,
                    bracket.Title,
                    bracket.ChannelId,
                    [EntryDto.FromEntry(match.Left!), EntryDto.FromEntry(match.Right!)]
                )
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
                    EntryDto.FromEntry(match.Left!),
                    EntryDto.FromEntry(match.Right!),
                    match.Title,
                    bracket.Title,
                    bracket.ChannelId
                )
            );

            await db.SaveChangesAsync();

            var nextMatches = db.GetMatchesToStart(bracket.Id).ToList();

            // Make sure we flag the matches before introducing delays
            foreach (var nextMatch in nextMatches)
                nextMatch.State = MatchState.Active;
            await db.SaveChangesAsync();

            foreach (var nextMatch in nextMatches)
            {
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
                        bracket.IsAnonymous,
                        bracket.RoleId,
                        bracket.ChannelId,
                        bracket.MatchDuration
                    )
                );

                await Task.Delay(TimeSpan.FromSeconds(3)); // Give each match breathing room
            }
        }
    }
}

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

        var match = bracket.Matches.First(m => m.Id == @event.MatchId);
        if (match.State is not MatchState.Active)
            return;

        match.State = MatchState.Complete;
        match.WinnerId = @event.Result is PollResult.Left ? match.LeftId : match.RightId;
        var winner = @event.Result is PollResult.Left ? match.Left : match.Right;

        if (match.NextId is null) // Final
        {
            logger.LogInformation("Champion picked for bracket {BracketId}", bracket.Id);
            bracket.State = MatchState.Complete;
            await eventBus.PublishAsync(
                new AnnounceChampionEvent(
                    bracket.Id,
                    bracket.Title,
                    bracket.ChannelId,
                    [EntryDto.FromEntry(winner!)]
                )
            );
        }
        else
        {
            logger.LogInformation(
                "Seeding winner of match {MatchId} into {NextMatchId}",
                match.Id,
                match.NextId
            );

            var next = await db.Matches.FindAsync(match.NextId);
            if (next is null)
                throw new ArgumentException($"Match with id {match.NextId} not found");

            if (match.SeedsLeftOfNext)
                next.LeftId = match.WinnerId;
            else
                next.RightId = match.WinnerId;

            await db.SaveChangesAsync();

            var freeMatchSlots =
                bracket.MaxConcurrentMatches
                - bracket.Matches.Count(m => m.State is MatchState.Active);

            var nextMatches = await db
                .Matches.Where(m => m.BracketId == bracket.Id)
                .Where(m => m.LeftId != null && m.RightId != null && m.State == MatchState.Pending)
                .OrderBy(m => m.Round)
                .ThenBy(m => m.Index)
                .Take(Math.Max(0, freeMatchSlots))
                .ToListAsync();

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
                        bracket.IsAnonymous,
                        bracket.RoleId,
                        bracket.ChannelId,
                        bracket.MatchDuration
                    )
                );
            }
        }
        await db.SaveChangesAsync();
    }
}

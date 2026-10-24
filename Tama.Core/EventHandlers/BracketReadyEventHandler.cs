// SPDX-License-Identifier: MPL-2.0

using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tama.Core.Dtos;
using Tama.Core.Events;
using Tama.Domain.Entities;
using Tama.Domain.Enums;
using Tama.Domain.ValueObjects;

namespace Tama.Core.EventHandlers;

public sealed class BracketReadyEventHandler(
    TamaDbContext db,
    IEventBus eventBus,
    ILogger<BracketReadyEventHandler> logger
) : IEventHandler<BracketReadyEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(BracketReadyEvent @event)
    {
        var bracket = await db
            .Brackets.Include(b => b.Matches)
            .FirstOrDefaultAsync(b => b.Id == @event.BracketId);

        if (bracket is null)
            return;

        // Start initial matches
        var matches = await db
            .Matches.Where(m => m.BracketId == bracket.Id)
            .Where(m => m.Left != null && m.Right != null)
            .Where(m => m.State == MatchState.Pending)
            .OrderBy(m => m.Round)
            .ThenBy(m => m.Index)
            .Take(bracket.MaxConcurrentMatches)
            .ToListAsync();

        foreach (var match in matches)
        {
            match.State = MatchState.Active;
            await eventBus.PublishAsync(
                new MatchStartedEvent(
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
                    bracket.DisplaySubmitters,
                    bracket.ChannelId,
                    bracket.MatchDuration
                )
            );
        }

        await db.SaveChangesAsync();
    }
}

// SPDX-License-Identifier: MPL-2.0

using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tama.Core.Events;
using Tama.Domain.Entities;
using Tama.Domain.Enums;
using Tama.Domain.ValueObjects;

namespace Tama.Core.EventHandlers;

public sealed class EntrySubmittedEventHandler(
    TamaDbContext db,
    IEventBus eventBus,
    ILogger<EntrySubmittedEventHandler> logger
) : IEventHandler<EntrySubmittedEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(EntrySubmittedEvent @event)
    {
        var bracket = await db
            .Brackets.Include(b => b.Entries)
            .Include(b => b.Matches)
            .FirstOrDefaultAsync(b => b.Id == @event.Bracket.Id);

        if (bracket is null)
            return;

        if (bracket.Entries.Count < bracket.EntryLimit)
            return;

        logger.LogInformation("Seeding bracket {BracketId}", bracket.Id);

        // We are ready to begin seeding the bracket
        var shuffledEntries = bracket.Entries.ToArray();
        new Random().Shuffle(shuffledEntries);

        bracket.Matches = BuildEmptyMatchTree(bracket.Id, bracket.EntryLimit);

        var round0 = bracket.Matches.Where(m => m.Round == 0).OrderBy(m => m.Index).ToList();
        for (var i = 0; i < round0.Count; i++)
        {
            var left = shuffledEntries[2 * i];
            var right = shuffledEntries[2 * i + 1];
            left.Seed = 2 * i + 1;
            right.Seed = 2 * i + 2;
            round0[i].Left = left;
            round0[i].Right = right;
        }

        bracket.State = MatchState.Active;
        await db.SaveChangesAsync();

        logger.LogInformation("Bracket {BracketId} is ready", bracket.Id);

        await eventBus.PublishAsync(new BracketReadyEvent(bracket.Id));
    }

    private static List<Match> BuildEmptyMatchTree(BracketId bracketId, int entryLimit)
    {
        List<Match> matches = [];
        var rounds = BitOperations.Log2((uint)entryLimit);

        for (var round = 0; round < rounds; round++)
        {
            var count = entryLimit >> (round + 1);
            for (var i = 0; i < count; i++)
            {
                var title = (rounds - 1 - round) switch
                {
                    0 => "Final",
                    1 => $"Semifinals (Match {i + 1} of {count})",
                    _ => $"Round {round + 1} (Match {i + 1} of {count})",
                };

                matches.Add(
                    new Match
                    {
                        BracketId = bracketId,
                        Round = round,
                        Index = i,
                        Title = title,
                        State = MatchState.Pending,
                    }
                );
            }
        }

        // Set up Next linking
        var byPos = matches.ToDictionary(m => (m.Round, m.Index));
        foreach (var m in matches)
        {
            if (byPos.TryGetValue((m.Round + 1, m.Index / 2), out var next))
                m.Next = next;
        }

        return matches;
    }
}

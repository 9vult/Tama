// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Tama.Domain.Entities;
using Tama.Domain.Enums;
using Tama.Domain.ValueObjects;

namespace Tama.Core;

public static class TamaDbContextExtensions
{
    public static IEnumerable<Match> GetMatchesToStart(this TamaDbContext db, BracketId bracketId)
    {
        var bracket = db
            .Brackets.Include(b => b.Matches)
            .Select(b => new
            {
                b.Id,
                b.MaxConcurrentMatches,
                b.Matches,
            })
            .FirstOrDefault(b => b.Id == bracketId);

        if (bracket is null)
            yield break;

        var freeMatchSlots =
            bracket.MaxConcurrentMatches - bracket.Matches.Count(m => m.State is MatchState.Active);
        if (freeMatchSlots <= 0)
            yield break;

        var currentRound = bracket
            .Matches.OrderBy(m => m.Round)
            .FirstOrDefault(m => m.State is not MatchState.Complete)
            ?.Round;

        // No matches to start
        if (currentRound is null)
            yield break;

        var matches = bracket
            .Matches.Where(m => m.Round == currentRound && m.IsReady)
            .OrderBy(m => m.Index)
            .Take(freeMatchSlots);

        foreach (var match in matches)
            yield return match;
    }
}

// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Tama.Domain.Enums;
using static Tama.Core.Features.Result<Tama.Core.Features.Queries.Match.GetNextMatchResponse>;

namespace Tama.Core.Features.Queries.Match;

public sealed class GetNextMatchHandler(ReadOnlyTamaDbContext db)
    : IQueryHandler<GetNextMatchQuery, Result<GetNextMatchResponse>>
{
    /// <inheritdoc />
    public async Task<Result<GetNextMatchResponse>> HandleAsync(GetNextMatchQuery query)
    {
        var bracket = await db
            .Brackets.Include(b => b.Matches)
            .FirstOrDefaultAsync(b => b.Id == query.BracketId);
        if (bracket is null)
            return Fail(ResultStatus.NoBracket);

        if (bracket.State is not MatchState.Active)
            return Fail(ResultStatus.BadRequest);

        var ongoingCount = bracket.Matches.Count(m => m.State is MatchState.Active);
        if (ongoingCount >= bracket.MaxConcurrentMatches)
            return Success(new GetNextMatchResponse(null));

        var nextMatch = bracket
            .Matches.OrderBy(m => m.Round)
            .ThenBy(m => m.Index)
            .FirstOrDefault(m => m.State is MatchState.Pending);
        return Success(new GetNextMatchResponse(nextMatch?.Id));
    }
}

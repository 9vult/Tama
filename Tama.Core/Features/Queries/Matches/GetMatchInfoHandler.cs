// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Tama.Core.Dtos;
using static Tama.Core.Features.Result<Tama.Core.Features.Queries.Matches.GetMatchInfoResponse>;

namespace Tama.Core.Features.Queries.Matches;

public sealed class GetMatchInfoHandler(ReadOnlyTamaDbContext db)
    : IQueryHandler<GetMatchInfoQuery, Result<GetMatchInfoResponse>>
{
    /// <inheritdoc />
    public async Task<Result<GetMatchInfoResponse>> HandleAsync(GetMatchInfoQuery query)
    {
        var result = await db
            .Matches.Where(m => m.Id == query.MatchId)
            .Select(m => new GetMatchInfoResponse(
                m.Bracket.Title,
                m.Title,
                m.Left != null
                    ? new EntryDto(
                        m.Left.Id,
                        m.Left.SubmitterId,
                        m.Left.Title,
                        m.Left.ImageUrl,
                        m.Left.SubmissionUrl
                    )
                    : null,
                m.Right != null
                    ? new EntryDto(
                        m.Right!.Id,
                        m.Right.SubmitterId,
                        m.Right.Title,
                        m.Right.ImageUrl,
                        m.Right.SubmissionUrl
                    )
                    : null
            ))
            .FirstOrDefaultAsync();

        return result is not null ? Success(result) : Fail(ResultStatus.Error);
    }
}

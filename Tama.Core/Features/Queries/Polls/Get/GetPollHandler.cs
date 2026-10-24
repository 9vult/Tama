// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using static Tama.Core.Features.Result<Tama.Core.Features.Queries.Polls.Get.GetPollResponse>;

namespace Tama.Core.Features.Queries.Polls.Get;

public sealed class GetPollHandler(ReadOnlyTamaDbContext db)
    : IQueryHandler<GetPollQuery, Result<GetPollResponse>>
{
    /// <inheritdoc />
    public async Task<Result<GetPollResponse>> HandleAsync(GetPollQuery query)
    {
        var poll = await db.Polls.FirstOrDefaultAsync(p => p.MessageId == query.MessageId);

        return poll is not null
            ? Success(new GetPollResponse(poll.MessageId, poll.MatchId))
            : Fail(ResultStatus.Error);
    }
}

// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using static Tama.Core.Features.Result<Tama.Core.Features.Queries.PollMessages.Get.GetPollMessageResponse>;

namespace Tama.Core.Features.Queries.PollMessages.Get;

public sealed class GetPollMessageHandler(ReadOnlyTamaDbContext db)
    : IQueryHandler<GetPollMessageQuery, Result<GetPollMessageResponse>>
{
    /// <inheritdoc />
    public async Task<Result<GetPollMessageResponse>> HandleAsync(GetPollMessageQuery query)
    {
        var poll = await db.PollMessages.FirstOrDefaultAsync(p => p.MessageId == query.MessageId);

        return poll is not null
            ? Success(new GetPollMessageResponse(poll.MessageId, poll.MatchId))
            : Fail(ResultStatus.Error);
    }
}

// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using static Tama.Core.Features.Result<Tama.Core.Features.Queries.BracketMessages.Get.GetBracketMessageResponse>;

namespace Tama.Core.Features.Queries.BracketMessages.Get;

public sealed class GetBracketMessageHandler(ReadOnlyTamaDbContext db)
    : IQueryHandler<GetBracketMessageQuery, Result<GetBracketMessageResponse>>
{
    /// <inheritdoc />
    public async Task<Result<GetBracketMessageResponse>> HandleAsync(GetBracketMessageQuery query)
    {
        var poll = await db.BracketMessages.FirstOrDefaultAsync(p =>
            p.MessageId == query.MessageId
        );

        return poll is not null
            ? Success(new GetBracketMessageResponse(poll.MessageId, poll.BracketId))
            : Fail(ResultStatus.Error);
    }
}

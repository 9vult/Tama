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
        var bracketMsg = await db.BracketMessages.FirstOrDefaultAsync(p =>
            p.BracketId == query.BracketId
        );

        if (bracketMsg is null)
            return Fail(ResultStatus.Error);

        var channelId = (await db.Brackets.FindAsync(bracketMsg.BracketId))?.ChannelId;

        return channelId is not null
            ? Success(
                new GetBracketMessageResponse(
                    bracketMsg.MessageId,
                    channelId.Value,
                    bracketMsg.BracketId
                )
            )
            : Fail(ResultStatus.Error);
    }
}

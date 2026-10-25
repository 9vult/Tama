// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.WebSocket;
using Tama.Core.Events;
using Tama.Core.Features.Queries.PollMessages.Get;
using Tama.Domain.Enums;
using Tama.Domain.ValueObjects;

namespace Tama.Discord.Handlers;

public sealed class MessageUpdateHandler(
    DiscordSocketClient client,
    IEventBus eventBus,
    GetPollMessageHandler getPollHandler
)
{
    public Task InitializeAsync()
    {
        client.MessageUpdated += OnMessageUpdatedAsync;
        return Task.CompletedTask;
    }

    private async Task OnMessageUpdatedAsync(
        Cacheable<IMessage, ulong> _1,
        SocketMessage after,
        ISocketMessageChannel _2
    )
    {
        if (after is not IUserMessage message)
            return;
        if (message.Author.Id != client.CurrentUser.Id)
            return;
        if (message.Poll?.Results is not { IsFinalized: true } results)
            return;

        var pollInfo = await getPollHandler.HandleAsync(
            new GetPollMessageQuery(MessageId.From(message.Id))
        );
        if (!pollInfo.IsSuccess)
            return;

        var leftCount = results.AnswerCounts.FirstOrDefault(a => a.AnswerId == 1).Count;
        var rightCount = results.AnswerCounts.FirstOrDefault(a => a.AnswerId == 2).Count;
        if (leftCount != rightCount)
        {
            var result = leftCount > rightCount ? PollResult.Left : PollResult.Right;
            await eventBus.PublishAsync(new MatchEndedEvent(pollInfo.Value.MatchId, result));
        }
        else
        {
            await eventBus.PublishAsync(new MatchTiedEvent(pollInfo.Value.MatchId));
        }
    }
}

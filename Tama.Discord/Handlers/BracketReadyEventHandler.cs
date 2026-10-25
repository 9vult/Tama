// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.WebSocket;
using Tama.Core.Events;
using Tama.Core.Features.Queries.BracketMessages.Get;

namespace Tama.Discord.Handlers;

public sealed class BracketReadyEventHandler(
    DiscordSocketClient client,
    GetBracketMessageHandler getBracketMessageHandler,
    ILogger<BracketReadyEventHandler> logger
) : IEventHandler<BracketReadyEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(BracketReadyEvent @event)
    {
        var bracketMessageResult = await getBracketMessageHandler.HandleAsync(
            new GetBracketMessageQuery(@event.BracketId)
        );
        if (!bracketMessageResult.IsSuccess)
        {
            logger.LogError("Failed to find message for bracket {BracketId}", @event.BracketId);
            return;
        }

        var info = bracketMessageResult.Value;

        if (await client.GetChannelAsync(info.ChannelId.Value) is not SocketTextChannel channel)
        {
            logger.LogError("Discord channel {ChannelId} was not found", info.ChannelId);
            return;
        }

        if (await channel.GetMessageAsync(info.MessageId.Value) is not IUserMessage message)
        {
            logger.LogError(
                "Discord message {MessageId} was not found in channel {ChannelId}",
                info.MessageId,
                info.ChannelId
            );
            return;
        }

        await message.ModifyAsync(msg =>
        {
            msg.Components = null;
        });
    }
}

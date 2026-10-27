// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.WebSocket;
using Tama.Core.Events;
using Tama.Core.Features.Queries.BracketMessages.Get;

namespace Tama.Discord.Handlers;

public sealed class EntrySubmittedEventHandler(
    DiscordSocketClient client,
    GetBracketMessageHandler getBracketMessageHandler,
    ILogger<EntrySubmittedEventHandler> logger
) : IEventHandler<EntrySubmittedEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(EntrySubmittedEvent @event)
    {
        var bracketMessageResult = await getBracketMessageHandler.HandleAsync(
            new GetBracketMessageQuery(@event.Bracket.Id)
        );
        if (!bracketMessageResult.IsSuccess)
        {
            logger.LogError("Failed to find message for bracket {BracketId}", @event.Bracket.Id);
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
            msg.Embed = BracketEmbedHelper.CreateEmbed(
                @event.Bracket,
                @event.CurrentSubmissionCount
            );
        });
    }
}

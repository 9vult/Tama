// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.WebSocket;
using Tama.Core.Events;

namespace Tama.Discord.Handlers;

public sealed class TiebreakRequestedEventHandler(
    DiscordSocketClient client,
    ILogger<TiebreakRequestedEventHandler> logger
) : IEventHandler<TiebreakRequestedEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(TiebreakRequestedEvent @event)
    {
        if (await client.GetChannelAsync(@event.ChannelId.Value) is not SocketTextChannel channel)
        {
            logger.LogError("Discord channel {ChannelId} was not found", @event.ChannelId);
            return;
        }

        if (await channel.GetMessageAsync(@event.MessageId.Value) is not IUserMessage message)
        {
            logger.LogError(
                "Discord message {MessageId} was not found in channel {ChannelId}",
                @event.MessageId,
                @event.ChannelId
            );
            return;
        }

        var leftId = $"tama:tiebreak:{@event.MatchId}:left";
        var rightId = $"tama:tiebreak:{@event.MatchId}:right";

        var component = new ComponentBuilderV2()
            .WithActionRow(builder =>
                builder
                    .WithButton(
                        emote: Emoji.Parse(":one:"),
                        customId: leftId,
                        style: ButtonStyle.Primary
                    )
                    .WithButton(
                        emote: Emoji.Parse(":two:"),
                        customId: rightId,
                        style: ButtonStyle.Primary
                    )
            )
            .Build();

        await message.ModifyAsync(msg =>
        {
            msg.Components = component;
        });
    }
}

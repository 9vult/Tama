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

        var leftId = $"tama.tiebreak.left:{@event.MatchId}";
        var rightId = $"tama.tiebreak.right:{@event.MatchId}";

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

        var embed = new EmbedBuilder()
            .WithTitle(@event.BracketTitle)
            .WithDescription(
                $"""
                **Tiebreak!**
                {@event.MatchTitle}

                Which should win?
                :one: {@event.Left.Title}
                :two: {@event.Right.Title}
                """
            )
            .Build();

        await channel.SendMessageAsync(embed: embed, components: component);
    }
}

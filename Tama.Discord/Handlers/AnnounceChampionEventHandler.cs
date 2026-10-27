// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.WebSocket;
using Tama.Core.Events;

namespace Tama.Discord.Handlers;

public sealed class AnnounceChampionEventHandler(
    DiscordSocketClient client,
    ILogger<AnnounceChampionEventHandler> logger
) : IEventHandler<AnnounceChampionEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(AnnounceChampionEvent @event)
    {
        if (await client.GetChannelAsync(@event.ChannelId.Value) is not SocketTextChannel channel)
        {
            logger.LogError("Discord channel {ChannelId} was not found", @event.ChannelId);
            return;
        }

        var description = new StringBuilder();
        var isSingleChamp = @event.Champions.Count == 1;

        description.AppendLine($"# We have a {(isSingleChamp ? "winner" : "tie")}!");
        description.AppendLine();
        description.AppendLine(
            $"The {(isSingleChamp ? "winner" : "winners")} of the **{@event.BracketTitle}** bracket {(isSingleChamp ? "is" : "are")}..."
        );
        foreach (var champ in @event.Champions)
            description.AppendLine($":crown: <@{champ.SubmitterId}>: **{champ.Title}**");

        var buttonId = $"tama.view:{@event.BracketId}:-1";
        var viewEntriesButton = new ComponentBuilder()
            .WithButton("View Entries", buttonId, emote: Emoji.Parse(":card_box:"))
            .Build();

        try
        {
            await channel.SendMessageAsync(description.ToString(), components: viewEntriesButton);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An error occured when processing champions for {BracketId} in channel {ChannelId}",
                @event.BracketId,
                @event.ChannelId
            );
        }
    }
}

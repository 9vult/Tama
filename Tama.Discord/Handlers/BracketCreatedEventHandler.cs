// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.WebSocket;
using Tama.Core.Events;
using Tama.Core.Features.Commands.BracketMessages.Create;
using Tama.Domain.Enums;
using Tama.Domain.ValueObjects;

namespace Tama.Discord.Handlers;

public sealed class BracketCreatedEventHandler(
    DiscordSocketClient client,
    CreateBracketMessageHandler createBracketMsgHandler,
    ILogger<BracketCreatedEventHandler> logger
) : IEventHandler<BracketCreatedEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(BracketCreatedEvent @event)
    {
        var bracket = @event.BracketDto;
        if (await client.GetChannelAsync(bracket.ChannelId.Value) is not SocketTextChannel channel)
        {
            logger.LogError("Discord channel {ChannelId} was not found", bracket.ChannelId);
            return;
        }

        var embed = BracketEmbedHelper.CreateEmbed(bracket, 0);

        var buttonId = $"tama.submit:{bracket.Id}";
        var component = new ComponentBuilderV2()
            .WithActionRow(builder =>
                builder.WithButton(
                    emote: Emoji.Parse(":ballot_box:"),
                    label: "Submit Entries",
                    customId: buttonId,
                    style: ButtonStyle.Primary
                )
            )
            .Build();

        try
        {
            var message = await channel.SendMessageAsync(embed: embed, components: component);

            logger.LogInformation(
                "Posted bracket {BracketId} in channel {ChannelId}",
                bracket.Id,
                bracket.ChannelId
            );

            await createBracketMsgHandler.HandleAsync(
                new CreateBracketMessageCommand(bracket.Id, MessageId.From(message.Id))
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An error occured when processing bracket {BracketId} in channel {ChannelId}",
                bracket.Id,
                bracket.ChannelId
            );
        }
    }
}

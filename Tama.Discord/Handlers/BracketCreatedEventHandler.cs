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

        var description = new StringBuilder();
        description.AppendLine("Bracket Settings:");
        description.AppendLine($"Entry limit: {bracket.EntryLimit}");
        description.Append("Match duration: ");
        description.AppendLine(
            bracket.MatchDuration switch
            {
                MatchDuration.OneHour => "1 hour",
                MatchDuration.FourHours => "4 hours",
                MatchDuration.EightHours => "8 hours",
                MatchDuration.OneDay => "1 day",
                MatchDuration.ThreeDays => "3 days",
                MatchDuration.OneWeek => "1 week",
                MatchDuration.ThreeWeeks => "3 weeks",
                _ => "Unknown",
            }
        );
        description.AppendLine();
        description.AppendLine(bracket.AllowOpenings ? ":o: Openings" : ":x: Openings");
        description.AppendLine(bracket.AllowEndings ? ":o: Endings" : ":x: Endings");
        description.AppendLine(bracket.AllowInserts ? ":o: Inserts" : ":x: Inserts");
        description.AppendLine();
        description.AppendLine(
            bracket.IsAnonymous
                ? ":disguised_face: Matches are anonymous"
                : ":star_struck: Matches are not anonymous"
        );
        description.AppendLine("*Overall winner is not anonymous*");

        var embed = new EmbedBuilder()
            .WithTitle(bracket.Title)
            .WithDescription(description.ToString())
            .WithCurrentTimestamp()
            .Build();

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

        logger.LogInformation("");

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

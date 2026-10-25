// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.WebSocket;
using Tama.Core.Events;
using Tama.Core.Features.Commands.PollMessages.Create;
using Tama.Domain.Enums;
using Tama.Domain.ValueObjects;

namespace Tama.Discord.Handlers;

public sealed class MatchStartedEventHandler(
    DiscordSocketClient client,
    CreatePollMessageHandler createPollHandler,
    ILogger<MatchStartedEventHandler> logger
) : IEventHandler<MatchStartedEvent>
{
    /// <inheritdoc />
    public async Task HandleAsync(MatchStartedEvent @event)
    {
        if (await client.GetChannelAsync(@event.ChannelId.Value) is not SocketTextChannel channel)
        {
            logger.LogError("Discord channel {ChannelId} was not found", @event.ChannelId);
            return;
        }

        var description = new StringBuilder();
        description.AppendLine($"**{@event.BracketTitle}** - {@event.MatchTitle}");
        description.AppendLine();

        if (@event.DisplaySubmitters)
            description.AppendLine(
                $"<@{@event.Left.SubmitterId}> vs <@{@event.Right.SubmitterId}>"
            );

        description.Append($"[{@event.Left.Title}](<{@event.Left.SubmissionUrl}>)");
        description.Append(" vs ");
        description.Append($"[{@event.Right.Title}](<{@event.Right.SubmissionUrl}>)");

        var components = new ComponentBuilderV2()
            .WithTextDisplay(description.ToString())
            .WithMediaGallery([@event.Left.ImageUrl, @event.Right.ImageUrl])
            .Build();

        var poll = new PollProperties
        {
            Question = new PollMediaProperties { Text = "Pick your favorite" },
            Duration = @event.MatchDuration switch
            {
                MatchDuration.OneHour => 1,
                MatchDuration.FourHours => 4,
                MatchDuration.EightHours => 8,
                MatchDuration.OneDay => 24,
                MatchDuration.ThreeDays => 72,
                MatchDuration.OneWeek => 168,
                MatchDuration.ThreeWeeks => 504,
                _ => 24,
            },
            Answers =
            [
                new PollMediaProperties { Text = @event.Left.Title },
                new PollMediaProperties { Text = @event.Right.Title },
            ],
            AllowMultiselect = false,
            LayoutType = PollLayout.Default,
        };

        try
        {
            // Description
            await channel.SendMessageAsync(
                components: components,
                allowedMentions: new AllowedMentions(AllowedMentionTypes.None)
            );

            // Poll
            var pollMessage = await channel.SendMessageAsync(poll: poll);
            await createPollHandler.HandleAsync(
                new CreatePollMessageCommand(@event.MatchId, MessageId.From(pollMessage.Id))
            );

            // Open thread
            await channel.CreateThreadAsync(@event.MatchTitle, message: pollMessage);

            logger.LogInformation(
                "Posted match {MatchId} in channel {ChannelId}",
                @event.MatchId,
                @event.ChannelId
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An error occured when processing match {MatchId} in channel {ChannelId}",
                @event.MatchId,
                @event.ChannelId
            );
        }
    }
}

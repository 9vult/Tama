// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Tama.Core.Events;
using Tama.Core.Features;
using Tama.Core.Features.Commands.Brackets.Create;
using Tama.Core.Features.Queries.PollMessages.Get;
using Tama.Domain;
using Tama.Domain.Enums;
using Tama.Domain.ValueObjects;
using RuntimeResult = Discord.Interactions.RuntimeResult;

namespace Tama.Discord.Interactions;

public sealed class CheckPollSlashCommand(
    DiscordSocketClient client,
    GetPollMessageHandler getPollHandler,
    IEventBus eventBus,
    ILogger<CreateBracketSlashCommand> logger
) : InteractionModuleBase<IInteractionContext>
{
    [SlashCommand("check-poll", "Force Tama to evaluate a poll")]
    public async Task<RuntimeResult> CheckPollAsync(
        [ChannelTypes(ChannelType.Text, ChannelType.News)] IMessageChannel channel,
        ulong messageId
    )
    {
        var interaction = Context.Interaction;
        var guild = client.GetGuild(interaction.GuildId!.Value);
        var member = guild.GetUser(interaction.User.Id);

        if (!member.GuildPermissions.Administrator)
        {
            await interaction.FollowupAsync("Sorry, only server admins can check polls!");
            return ExecutionResult.Success;
        }

        // Check bot permissions on the specified channel
        if (channel is not SocketTextChannel guildChannel)
        {
            logger.LogError("Invalid channel {ChannelId}", channel.Id);
            return ExecutionResult.Failure;
        }

        if (await guildChannel.GetMessageAsync(messageId) is not IUserMessage message)
        {
            await interaction.FollowupAsync("Invalid message");
            return ExecutionResult.Success;
        }

        if (message.Author.Id != client.CurrentUser.Id)
        {
            await interaction.FollowupAsync("Tama-chan didn't send that message!");
            return ExecutionResult.Success;
        }

        if (message.Poll is null)
        {
            await interaction.FollowupAsync("That doesn't look like a poll to Tama-chan...");
            return ExecutionResult.Success;
        }

        if (message.Poll.Value.Results is null)
        {
            await interaction.FollowupAsync("That poll isn't done yet!");
            return ExecutionResult.Success;
        }

        var pollInfo = await getPollHandler.HandleAsync(
            new GetPollMessageQuery(MessageId.From(message.Id))
        );
        if (!pollInfo.IsSuccess)
        {
            await interaction.FollowupAsync("That doesn't look like a poll to Tama-chan...");
            return ExecutionResult.Success;
        }

        var results = message.Poll.Value.Results.Value;

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

        // Lock the thread
        if (message.Thread is not null)
        {
            await message.Thread.ModifyAsync(thread =>
            {
                thread.Locked = true;
                thread.Archived = true;
            });
        }

        await interaction.FollowupAsync("Done! Thanks for letting Tama-chan know about that.");
        return ExecutionResult.Success;
    }
}

// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Tama.Core.Features;
using Tama.Core.Features.Commands.Brackets.Create;
using Tama.Domain;
using Tama.Domain.Enums;
using Tama.Domain.ValueObjects;

namespace Tama.Discord.Interactions;

public sealed class CreateBracketSlashCommand(
    DiscordSocketClient client,
    CreateBracketHandler createBracketHandler,
    ILogger<CreateBracketSlashCommand> logger
) : InteractionModuleBase<IInteractionContext>
{
    [SlashCommand("create-bracket", "Create a new bracket")]
    public async Task<RuntimeResult> CreateBracketAsync(
        [MaxLength(Length.Title)] string title,
        [ChannelTypes(ChannelType.Text, ChannelType.News)] IMessageChannel channel,
        EntryLimitChoices entryLimit,
        bool allowOpenings,
        bool allowEndings,
        bool isAnonymous,
        bool allowFinalTie,
        int maxConcurrentMatches = 2,
        MatchDurationChoices matchDuration = MatchDurationChoices.OneDay
    )
    {
        title = title.Trim();
        var interaction = Context.Interaction;
        var guild = client.GetGuild(interaction.GuildId!.Value);
        var member = guild.GetUser(interaction.User.Id);

        if (!member.GuildPermissions.Administrator)
        {
            await interaction.FollowupAsync("Sorry, only server admins can create brackets!");
            return ExecutionResult.Success;
        }

        // Check bot permissions on the specified channel
        if (channel is not SocketTextChannel guildChannel)
        {
            logger.LogError("Invalid channel {ChannelId}", channel.Id);
            return ExecutionResult.Failure;
        }

        if (!CheckPermissions())
        {
            await interaction.FollowupAsync(
                """
                Tama-chan doesn't have enough permissions to use that channel!
                Please make sure Tama-chan has the following permissions:
                - View Channel
                - Send Messages
                - Embed Links
                - Send Polls
                - Create Public Threads
                - Manage Threads
                """
            );
            return ExecutionResult.Success;
        }

        var command = new CreateBracketCommand(
            GroupId.From(guild.Id),
            ChannelId.From(channel.Id),
            UserId.From(member.Id),
            title,
            allowFinalTie,
            (int)entryLimit,
            maxConcurrentMatches,
            (MatchDuration)matchDuration,
            allowOpenings,
            allowEndings,
            false,
            isAnonymous
        );

        var result = await createBracketHandler.HandleAsync(command);
        if (result.IsSuccess)
        {
            await interaction.FollowupAsync($"Successfully created the bracket **{title}**!");
            return ExecutionResult.Success;
        }

        await interaction.FollowupAsync(
            result.Status switch
            {
                ResultStatus.BracketConflict => "A bracket with that name already exists!",
                ResultStatus.BadRequest => "EntryLimit is not a power of two",
                _ => "An unknown error occured.",
            }
        );
        return ExecutionResult.Success;

        bool CheckPermissions()
        {
            var guildUser = guildChannel.Guild.GetUser(client.CurrentUser.Id);
            var guildPerms = guildUser.GuildPermissions;
            var channelPerms = guildUser.GetPermissions(guildChannel);

            if (guildPerms.Administrator)
                return true;

            return (guildPerms.ViewChannel || channelPerms.ViewChannel)
                && (guildPerms.SendMessages || channelPerms.SendMessages)
                && (guildPerms.EmbedLinks || channelPerms.EmbedLinks)
                && (guildPerms.SendPolls || channelPerms.SendPolls)
                && (guildPerms.CreatePublicThreads || channelPerms.CreatePublicThreads)
                && (guildPerms.ManageThreads || channelPerms.ManageThreads);
        }
    }

    public enum EntryLimitChoices
    {
        [ChoiceDisplay("2")]
        Two = 2,

        [ChoiceDisplay("4")]
        Four = 4,

        [ChoiceDisplay("8")]
        Eight = 8,

        [ChoiceDisplay("16")]
        Sixteen = 16,

        [ChoiceDisplay("32")]
        ThirtyTwo = 32,

        [ChoiceDisplay("64")]
        SixtyFour = 64,
    }

    public enum MatchDurationChoices
    {
        [ChoiceDisplay("1 hour")]
        OneHour = 0,

        [ChoiceDisplay("4 hours")]
        FourHours = 1,

        [ChoiceDisplay("8 hours")]
        EightHours = 2,

        [ChoiceDisplay("1 day")]
        OneDay = 3,

        [ChoiceDisplay("3 days")]
        ThreeDays = 4,

        [ChoiceDisplay("1 week")]
        OneWeek = 5,

        [ChoiceDisplay("3 weeks")]
        ThreeWeeks = 6,
    }
}

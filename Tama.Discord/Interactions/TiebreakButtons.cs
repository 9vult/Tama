// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Tama.Core.Events;
using Tama.Core.Features.Queries.Matches;
using Tama.Domain.Enums;
using Tama.Domain.ValueObjects;

namespace Tama.Discord.Interactions;

public sealed class TiebreakButtons(
    DiscordSocketClient client,
    GetMatchInfoHandler matchInfoHandler,
    IEventBus eventBus,
    ILogger<TiebreakButtons> logger
) : InteractionModuleBase<IInteractionContext>
{
    [ComponentInteraction("tama.tiebreak.left:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> TiebreakLeftAsync(string rawId)
    {
        if (!MatchId.TryParse(rawId, out var matchId))
        {
            logger.LogError("Could not parse match id: {MatchId}", rawId);
            return ExecutionResult.Failure;
        }

        var interaction = Context.Interaction;
        var guild = client.GetGuild(interaction.GuildId!.Value);
        var member = guild.GetUser(interaction.User.Id);

        if (!member.GuildPermissions.Administrator)
        {
            await interaction.FollowupAsync(
                "Sorry, only server admins can break ties!",
                ephemeral: true
            );
            return ExecutionResult.Success;
        }

        await eventBus.PublishAsync(new TiebreakSubmittedEvent(matchId, PollResult.Left));
        await interaction.FollowupAsync("Your response has been recorded", ephemeral: true);

        var infoResult = await matchInfoHandler.HandleAsync(new GetMatchInfoQuery(matchId));
        if (!infoResult.IsSuccess)
        {
            logger.LogError("Failed to get info for match {MatchId}", matchId);
            return ExecutionResult.Failure;
        }
        var info = infoResult.Value;

        var embed = new EmbedBuilder()
            .WithTitle(info.BracketTitle)
            .WithDescription(
                $"""
                **Tiebreak!**
                {info.MatchTitle}

                :one: {info.Left?.Title ?? "Unknown"} :white_check_mark:
                :two: {info.Right?.Title ?? "Unknown"}
                """
            )
            .Build();

        await interaction.ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = embed;
            msg.Components = null;
        });
        return ExecutionResult.Success;
    }

    [ComponentInteraction("tama.tiebreak.right:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> TiebreakRightAsync(string rawId)
    {
        if (!MatchId.TryParse(rawId, out var matchId))
        {
            logger.LogError("Could not parse match id: {MatchId}", rawId);
            return ExecutionResult.Failure;
        }

        var interaction = Context.Interaction;
        var guild = client.GetGuild(interaction.GuildId!.Value);
        var member = guild.GetUser(interaction.User.Id);

        if (!member.GuildPermissions.Administrator)
        {
            await interaction.FollowupAsync(
                "Sorry, only server admins can break ties!",
                ephemeral: true
            );
            return ExecutionResult.Success;
        }

        await eventBus.PublishAsync(new TiebreakSubmittedEvent(matchId, PollResult.Right));
        await interaction.FollowupAsync("Your response has been recorded", ephemeral: true);

        var infoResult = await matchInfoHandler.HandleAsync(new GetMatchInfoQuery(matchId));
        if (!infoResult.IsSuccess)
        {
            logger.LogError("Failed to get info for match {MatchId}", matchId);
            return ExecutionResult.Failure;
        }
        var info = infoResult.Value;

        var embed = new EmbedBuilder()
            .WithTitle(info.BracketTitle)
            .WithDescription(
                $"""
                **Tiebreak!**
                {info.MatchTitle}

                :one: {info.Left?.Title ?? "Unknown"} 
                :two: {info.Right?.Title ?? "Unknown"} :white_check_mark:
                """
            )
            .Build();

        await interaction.ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = embed;
            msg.Components = null;
        });
        return ExecutionResult.Success;
    }
}

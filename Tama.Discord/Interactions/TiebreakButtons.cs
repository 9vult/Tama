// SPDX-License-Identifier: MPL-2.0

using Discord.Interactions;
using Discord.WebSocket;
using Tama.Core.Events;
using Tama.Domain.Enums;
using Tama.Domain.ValueObjects;

namespace Tama.Discord.Interactions;

public sealed class TiebreakButtons(
    DiscordSocketClient client,
    IEventBus eventBus,
    ILogger<TiebreakButtons> logger
) : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
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

        if (interaction.Message is null)
            return ExecutionResult.Success;

        await interaction.Message.ModifyAsync(msg =>
        {
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

        if (interaction.Message is null)
            return ExecutionResult.Success;

        await interaction.Message.ModifyAsync(msg =>
        {
            msg.Components = null;
        });
        return ExecutionResult.Success;
    }
}

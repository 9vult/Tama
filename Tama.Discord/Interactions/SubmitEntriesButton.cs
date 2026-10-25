// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Tama.Discord.Modals;
using Tama.Domain.ValueObjects;

namespace Tama.Discord.Interactions;

public sealed class SubmitEntriesButton(
    DiscordSocketClient client,
    ILogger<SubmitEntriesButton> logger
) : InteractionModuleBase<IInteractionContext>
{
    [ComponentInteraction("tama.submit:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> HandleClickAsync(string rawId)
    {
        if (!BracketId.TryParse(rawId, out var bracketId))
        {
            logger.LogError("Could not parse bracket id: {BracketId}", rawId);
            return ExecutionResult.Failure;
        }

        var interaction = Context.Interaction;
        var guild = client.GetGuild(interaction.GuildId!.Value);
        var member = guild.GetUser(interaction.User.Id);

        logger.LogInformation(
            "Displaying submission modal for bracket {BracketId} to user {UserId}",
            bracketId,
            member.Id
        );

        await interaction.RespondWithModalAsync<SubmissionModal>(
            $"tama.modal.submission:{bracketId}"
        );
        return ExecutionResult.Success;
    }
}

// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Tama.Core.Features;
using Tama.Core.Features.Commands.Entries.Submit;
using Tama.Discord.Modals;
using Tama.Domain.ValueObjects;

namespace Tama.Discord.Interactions;

public sealed class ModalSubmitted(
    DiscordSocketClient client,
    SubmitEntriesHandler submitEntriesHandler,
    ILogger<ModalSubmitted> logger
) : InteractionModuleBase<IInteractionContext>
{
    [ModalInteraction("tama.modal.submission:*")]
    public async Task HandleModalSubmission(string rawId, SubmissionModal modal)
    {
        if (!BracketId.TryParse(rawId, out var bracketId))
        {
            logger.LogError("Could not parse bracket id: {BracketId}", rawId);
            return;
        }

        var interaction = Context.Interaction;
        var guild = client.GetGuild(interaction.GuildId!.Value);
        var member = guild.GetUser(interaction.User.Id);

        if (
            !Uri.TryCreate(modal.FirstChoice, UriKind.Absolute, out var first)
            || !Uri.TryCreate(modal.SecondChoice, UriKind.Absolute, out var second)
            || !Uri.TryCreate(modal.ThirdChoice, UriKind.Absolute, out var third)
        )
        {
            await interaction.FollowupAsync(
                "One or more of your submissions was an invalid URL. Please try again.",
                ephemeral: true
            );
            return;
        }

        var randomize = modal.CheckedSettings.Contains("randomize");
        var notifyMe = modal.CheckedSettings.Contains("notify");

        var command = new SubmitEntriesCommand(
            bracketId,
            UserId.From(member.Id),
            first,
            second,
            third,
            randomize
        );

        var response = await submitEntriesHandler.HandleAsync(command);
        if (response.IsSuccess)
        {
            await interaction.FollowupAsync("Your response has been recorded.", ephemeral: true);

            if (notifyMe)
                await member.AddRoleAsync(response.Value.RoleId.Value);

            return;
        }

        await interaction.FollowupAsync(
            response.Status switch
            {
                ResultStatus.Error => "An AnimeThemes error occured. Please try again later.",
                ResultStatus.NoBracket => "That bracket does not exist",
                ResultStatus.BadRequest when response.Message is "TYPE" =>
                    "One or more of your submissions was a disallowed theme type. Please try agin.",
                ResultStatus.BadRequest when response.Message is "URL" =>
                    "One or more of your submissions was an invalid AnimeThemes.moe video page URL. Please try again.",
                ResultStatus.BadRequest => "That bracket is already in-progress",
                ResultStatus.EntryConflict =>
                    "All of your submissions already exist in the bracket. Please try again.",
                ResultStatus.AlreadySubmitted => "You have already submitted to this bracket!",
                _ => "An error occured while processing your submission",
            },
            ephemeral: true
        );
    }
}

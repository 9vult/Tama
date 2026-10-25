// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Tama.Core.Services;

namespace Tama.Discord.Interactions;

public sealed class AboutSlashCommand(VersionService versionService)
    : InteractionModuleBase<IInteractionContext>
{
    [SlashCommand("about", "About Tama")]
    public async Task<RuntimeResult> ShowAboutAsync()
    {
        var interaction = Context.Interaction;

        var body = new StringBuilder();
        body.AppendLine($"Version: {versionService.FullLabel}");
        body.AppendLine("Author: <@248600185423396866>");

        await interaction.FollowupAsync(
            embed: new EmbedBuilder()
                .WithAuthor(name: "Tama", url: "https://github.com/9vult/Tama")
                .WithThumbnailUrl("https://i.imgur.com/6qi5ufC.jpeg")
                .WithDescription(body.ToString())
                .WithCurrentTimestamp()
                .Build()
        );
        return ExecutionResult.Success;
    }
}

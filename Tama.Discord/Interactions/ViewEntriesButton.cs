// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Tama.Core.Features.Queries.Entries;
using Tama.Domain.ValueObjects;

namespace Tama.Discord.Interactions;

public sealed class ViewEntriesButton(
    DiscordSocketClient client,
    ViewEntriesHandler viewEntriesHandler,
    ILogger<ViewEntriesButton> logger
) : InteractionModuleBase<IInteractionContext>
{
    [ComponentInteraction("tama.view:*:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> HandleClickAsync(string rawId, int page)
    {
        // Initial state check so we can follow up instead of modifying the message
        var isInitialState = page < 0;
        if (page < 0)
            page = 0;

        if (!BracketId.TryParse(rawId, out var bracketId))
        {
            logger.LogError("Could not parse bracket id: {BracketId}", rawId);
            return ExecutionResult.Failure;
        }

        var interaction = Context.Interaction;
        await interaction.DeferAsync();

        var guild = client.GetGuild(interaction.GuildId!.Value);
        var member = guild.GetUser(interaction.User.Id);

        var result = await viewEntriesHandler.HandleAsync(new ViewEntriesQuery(bracketId, page));
        if (!result.IsSuccess)
        {
            await interaction.FollowupAsync("An error occured!", ephemeral: true);
            return ExecutionResult.Failure;
        }

        var data = result.Value;

        var b = new StringBuilder();
        foreach (var entry in data.Entries)
        {
            b.AppendLine($"<@{entry.SubmitterId}>: [{entry.Title}]({entry.SubmissionUrl})");
        }

        var embed = new EmbedBuilder()
            .WithTitle(data.BracketTitle)
            .WithDescription(b.ToString())
            .WithFooter($"Page {data.Page + 1} / {data.PageCount}")
            .Build();

        MessageComponent? component = null;
        if (data.PageCount != 1)
        {
            var prevId = $"tama.view:{bracketId}:{data.Page - 1}";
            var nextId = $"tama.view:{bracketId}:{data.Page + 1}";

            var hasPrev = data.Page > 0;
            var hasNext = data.Page < data.PageCount - 1;

            component = new ComponentBuilder()
                .WithButton("◀", prevId, disabled: !hasPrev)
                .WithButton("▶", nextId, disabled: !hasNext)
                .Build();
        }

        logger.LogInformation(
            "Displaying entry list for bracket {BracketId} to user {UserId}",
            bracketId,
            member.Id
        );

        if (isInitialState)
        {
            await interaction.FollowupAsync(embed: embed, components: component, ephemeral: true);
        }
        else
        {
            await interaction.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embed = embed;
                msg.Components = component;
            });
        }

        return ExecutionResult.Success;
    }
}

// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Tama.Core.Dtos;
using Tama.Domain.Enums;

namespace Tama.Discord;

public static class BracketEmbedHelper
{
    public static Embed CreateEmbed(BracketDto bracket, int currentSubmissionCount)
    {
        var description = new StringBuilder();

        if (!string.IsNullOrEmpty(bracket.Description))
        {
            description.AppendLine(bracket.Description);
            description.AppendLine();
        }

        description.AppendLine("Bracket Settings:");
        description.AppendLine($"- Entry limit: {bracket.EntryLimit}");
        description.Append("- Match duration: ");
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
        if (bracket.IsAnonymous)
            description.AppendLine("*Details will be revealed after a winner is determined.*");

        return new EmbedBuilder()
            .WithTitle(bracket.Title)
            .WithDescription(description.ToString())
            .WithFooter($"{currentSubmissionCount}/{bracket.EntryLimit}")
            .WithCurrentTimestamp()
            .Build();
    }
}

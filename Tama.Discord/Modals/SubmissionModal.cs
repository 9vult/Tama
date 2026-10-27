// SPDX-License-Identifier: MPL-2.0

using Discord.Interactions;
using Tama.Domain;

namespace Tama.Discord.Modals;

public sealed class SubmissionModal : IModal
{
    /// <inheritdoc />
    public string Title => "Submit Entries";

    [ModalTextDisplay]
    public static string Introduction =>
        """
            **Submissions must be AnimeThemes.moe video page links.**
            Example: <https://animethemes.moe/anime/clannad/ED1-NCBD1080>
            """;

    [RequiredInput]
    [InputLabel("First Choice")]
    [ModalTextInput("choice-1", maxLength: Length.Url)]
    public string FirstChoice { get; set; } = string.Empty;

    [RequiredInput]
    [InputLabel("Second Choice")]
    [ModalTextInput("choice-2", maxLength: Length.Url)]
    public string SecondChoice { get; set; } = string.Empty;

    [RequiredInput]
    [InputLabel("Third Choice")]
    [ModalTextInput("choice-3", maxLength: Length.Url)]
    public string ThirdChoice { get; set; } = string.Empty;

    [RequiredInput(false)]
    [InputLabel("Options")]
    [ModalCheckboxGroup(customId: "toggles", minValues: 0, maxValues: 2)]
    [ModalCheckboxGroupOption(
        "Randomize Order",
        "randomize",
        "Shuffle submissions",
        defaultState: false
    )]
    [ModalCheckboxGroupOption(
        "Notify Me",
        "notify",
        "Notifies you when it's time to vote",
        defaultState: true
    )]
    public string[] CheckedSettings { get; set; } = [];
}

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

            Please fill out your first, second, and third choice.
            If your first choice has already been submitted,
            your second choice will be picked, and so on.
            If all three of your choices have already been submitted,
            you can try again with new choices. 
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

    [InputLabel("Randomize Order")]
    [ModalCheckbox("randomize")]
    public bool Randomize { get; set; } = false;
}

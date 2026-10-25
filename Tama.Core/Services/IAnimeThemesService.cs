// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.Enums;

namespace Tama.Core.Services;

/// <summary>
/// Service for interacting with the AnimeThemes.moe API
/// </summary>
public interface IAnimeThemesService
{
    /// <summary>
    /// Validate that the given <paramref name="url"/> is a valid AnimeThemes.moe video page
    /// </summary>
    /// <param name="url">Url to check</param>
    /// <returns><see langword="true"/></returns>
    bool ValidateUrl(Uri url);

    /// <summary>
    /// Validate that the given <paramref name="url"/> is for a valid theme type
    /// </summary>
    /// <param name="url">Url to check</param>
    /// <param name="allowOpenings">If openings are allowed</param>
    /// <param name="allowEndings">If endings are allowed</param>
    /// <param name="allowInserts">If inserts are allowed</param>
    /// <returns></returns>
    bool ValidateType(Uri url, bool allowOpenings, bool allowEndings, bool allowInserts);

    /// <summary>
    /// Get the type of theme at the <paramref name="url"/>
    /// </summary>
    /// <remarks>Assumes <paramref name="url"/> is valid</remarks>
    /// <param name="url">AnimeThemes video page</param>
    /// <returns>The type of theme</returns>
    ThemeType GetThemeType(Uri url);

    /// <summary>
    /// Get the theme identifier
    /// </summary>
    /// <remarks>Assumes <paramref name="url"/> is valid</remarks>
    /// <param name="url">AnimeThemes video page</param>
    /// <returns>Theme identifier</returns>
    /// <example>OP2</example>
    string GetTheme(Uri url);

    /// <summary>
    /// Get the unique theme identifier
    /// </summary>
    /// <remarks>Assumes <paramref name="url"/> is valid</remarks>
    /// <param name="url">AnimeThemes video page</param>
    /// <returns>Unique theme identifier</returns>
    /// <example>clannad_after_story:OP1</example>
    string GetUniqueThemeId(Uri url);

    /// <summary>
    /// Get details about the anime
    /// </summary>
    /// <remarks>Assumes <paramref name="url"/> is valid</remarks>
    /// <param name="url">AnimeThemes video page</param>
    /// <returns>Anime details</returns>
    Task<AnimeDto> GetAnimeDetailsAsync(Uri url);
}

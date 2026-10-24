// SPDX-License-Identifier: MPL-2.0

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tama.Domain.Enums;

namespace Tama.Core.Services;

public class AnimeThemesService(HttpClient client, ILogger<AnimeThemesService> logger)
    : IAnimeThemesService
{
    private const string BaseUrl = "https://graphql.animethemes.moe";
    private const string ATHost = "animethemes.moe";
    private const string AnimeSegment = "anime/";
    private const string AniList = "ANILIST";

    private static readonly JsonSerializerOptions QueryJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <inheritdoc />
    public bool ValidateUrl(Uri url)
    {
        if (url.Host != ATHost)
            return false;

        if (url.Segments.Length < 4)
            return false;

        if (url.Segments[1] != AnimeSegment)
            return false;

        return GetThemeType(url) is not ThemeType.Unknown;
    }

    /// <inheritdoc />
    public ThemeType GetThemeType(Uri url)
    {
        if (url.Segments[3].StartsWith("OP"))
            return ThemeType.Opening;
        if (url.Segments[3].StartsWith("ED"))
            return ThemeType.Ending;
        if (url.Segments[3].StartsWith("INS"))
            return ThemeType.Insert;
        return ThemeType.Unknown;
    }

    /// <inheritdoc />
    public string GetTheme(Uri url)
    {
        var content = url.Segments[3];
        var hyIdx = content.IndexOf('-');
        return hyIdx == -1 ? content : content[..hyIdx];
    }

    /// <inheritdoc />
    public string GetUniqueThemeId(Uri url)
    {
        var anime = GetAnimeSlug(url);
        var theme = GetTheme(url);
        return $"{anime}:{theme}";
    }

    /// <inheritdoc />
    public async Task<AnimeDto> GetAnimeDetailsAsync(Uri url)
    {
        var response = await client.PostAsync(BaseUrl, CreateQuery(GetAnimeSlug(url)));
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(
                "AnimeThemes API returned status code {StatusCode}",
                response.StatusCode
            );
            return new AnimeDto { Title = string.Empty, AniListUrl = string.Empty };
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        if (doc.RootElement.TryGetProperty("errors", out var errors))
        {
            logger.LogError("AnimeThemes API returned errors: {Errors}", string.Join(',', errors));
            return new AnimeDto { Title = string.Empty, AniListUrl = string.Empty };
        }

        var anime = doc.RootElement.GetProperty("data").GetProperty("anime");
        var title = anime.GetProperty("title").GetProperty("romaji").GetString();
        var resourceCount = anime.GetProperty("resources").GetProperty("nodes").GetArrayLength();
        var aniListUrl =
            resourceCount > 0
                ? anime
                    .GetProperty("resources")
                    .GetProperty("nodes")[0]
                    .GetProperty("link")
                    .GetString()
                : string.Empty;

        return new AnimeDto
        {
            Title = title ?? string.Empty,
            AniListUrl = aniListUrl ?? string.Empty,
        };
    }

    private static string GetAnimeSlug(Uri url)
    {
        return url.Segments[2][..^1];
    }

    private static StringContent CreateQuery(string animeSlug)
    {
        return new StringContent(
            JsonSerializer.Serialize(
                new
                {
                    Query = """
                    query ($slug: String!, $site: ResourceSite) {
                      anime(slug: $slug) {
                        title {
                          romaji
                        }
                        resources(site: $site) {
                          nodes {
                            link
                          }
                        }
                      }
                    }
                    """,
                    Variables = new { Slug = animeSlug, Site = AniList },
                },
                QueryJsonOptions
            ),
            Encoding.UTF8,
            "application/json"
        );
    }
}

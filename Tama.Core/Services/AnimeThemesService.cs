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
    private const string LargeCover = "LARGE_COVER";

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
            return new AnimeDto { Title = string.Empty, ImageUrl = string.Empty };
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        if (doc.RootElement.TryGetProperty("errors", out var errors))
        {
            logger.LogError("AnimeThemes API returned errors: {Errors}", string.Join(',', errors));
            return new AnimeDto { Title = string.Empty, ImageUrl = string.Empty };
        }

        var anime = doc.RootElement.GetProperty("data").GetProperty("anime");
        var title = anime.GetProperty("title").GetProperty("romaji").GetString();
        var imageCount = anime.GetProperty("images").GetProperty("nodes").GetArrayLength();
        var imageUrl =
            imageCount > 0
                ? anime
                    .GetProperty("images")
                    .GetProperty("nodes")[0]
                    .GetProperty("link")
                    .GetString()
                : string.Empty;

        return new AnimeDto { Title = title ?? string.Empty, ImageUrl = imageUrl ?? string.Empty };
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
                    query ($slug: String!, $facet: ImageFacet) {
                      anime(slug: $slug) {
                        title {
                          romaji
                        }
                        images(facet: $facet) {
                          nodes {
                            link
                          }
                        }
                      }
                    }
                    """,
                    Variables = new { Slug = animeSlug, Facet = LargeCover },
                },
                QueryJsonOptions
            ),
            Encoding.UTF8,
            "application/json"
        );
    }
}

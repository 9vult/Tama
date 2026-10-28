// SPDX-License-Identifier: MPL-2.0

namespace Tama.Discord;

public static class StringExtensions
{
    /// <summary>
    /// Truncate a string to a <paramref name="maxLength"/>, excluding the <paramref name="suffix"/>
    /// </summary>
    /// <param name="value">String to truncate</param>
    /// <param name="maxLength">Maximum allowed length, excluding the suffix</param>
    /// <param name="suffix">Suffix, defaults to ellipsis character</param>
    /// <returns>Truncated string</returns>
    /// <example>"Hello".Truncate(5) → "Hello"</example>
    /// <example>"Hello".Truncate(4) → "Hell…"</example>
    public static string? Truncate(this string? value, int maxLength, string suffix = "…")
    {
        return value?.Length > maxLength ? value[..maxLength] + suffix : value;
    }
}

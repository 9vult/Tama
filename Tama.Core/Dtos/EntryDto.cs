// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.Entities;
using Tama.Domain.ValueObjects;

namespace Tama.Core.Dtos;

public sealed record EntryDto(
    EntryId Id,
    UserId SubmitterId,
    string Title,
    string ImageUrl,
    string SubmissionUrl
)
{
    public static EntryDto FromEntry(Entry entry)
    {
        return new EntryDto(
            entry.Id,
            entry.SubmitterId,
            entry.Title,
            entry.ImageUrl,
            entry.SubmissionUrl
        );
    }
}

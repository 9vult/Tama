// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.ValueObjects;

namespace Tama.Core.Dtos;

public sealed record EntryDto(
    EntryId Id,
    UserId SubmitterId,
    string Title,
    string ImageUrl,
    string SubmissionUrl
);

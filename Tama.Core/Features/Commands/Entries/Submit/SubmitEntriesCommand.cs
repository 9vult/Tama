// SPDX-License-Identifier: MPL-2.0

using Tama.Domain.ValueObjects;

namespace Tama.Core.Features.Commands.Entries.Submit;

public sealed record SubmitEntriesCommand(
    BracketId BracketId,
    UserId SubmitterId,
    Uri FirstSubmission,
    Uri SecondSubmission,
    Uri ThirdSubmission
) : ICommand;

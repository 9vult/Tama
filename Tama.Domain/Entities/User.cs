// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Tama.Domain.ValueObjects;

namespace Tama.Domain.Entities;

public sealed class User : MappedId<UserId>
{
    [MaxLength(Length.Name)]
    public string Name { get; set; } = string.Empty;

    public bool IsSystemAdministrator { get; set; } = false;
}

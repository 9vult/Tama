// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel.DataAnnotations;
using Tama.Domain.ValueObjects;

namespace Tama.Domain.Entities;

public sealed class Group : MappedId<GroupId>
{
    public UserId? OwnerId { get; set; }
    public User? Owner { get; set; }

    [MaxLength(Length.Name)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Bracket> Brackets { get; set; } = [];
}

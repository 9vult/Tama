// SPDX-License-Identifier: MPL-2.0

using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tama.Domain.Entities;
using Tama.Domain.ValueObjects;
using Vogen;

namespace Tama.Core;

public class TamaDbContext(DbContextOptions<TamaDbContext> options) : DbContext(options)
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public DbSet<Bracket> Brackets { get; init; } = null!;
    public DbSet<Match> Matches { get; init; } = null!;
    public DbSet<Entry> Entries { get; set; } = null!;
    public DbSet<Poll> Polls { get; set; } = null!;

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(TamaDbContext).Assembly);
    }

    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        base.ConfigureConventions(builder);

        // Vogen
        var voTypes = typeof(UserId)
            .Assembly.GetTypes()
            .Where(t => t.IsValueType && t.GetCustomAttribute<ValueObjectAttribute>() is not null);

        foreach (var type in voTypes)
        {
            var converterType = type.GetNestedType("EfCoreValueConverter");
            if (converterType is not null)
                builder.Properties(type).HaveConversion(converterType);
        }

        // Other converters
        builder.Properties<ulong>().HaveConversion<UlongStringConverter>();
    }

    private class UlongStringConverter()
        : ValueConverter<ulong, string>(
            v => v.ToString(CultureInfo.InvariantCulture),
            v => ulong.Parse(v, CultureInfo.InvariantCulture)
        );
}

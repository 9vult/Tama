// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tama.Core.Features.Commands;
using Tama.Core.Features.Queries;

namespace Tama.Core;

public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddCore(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHttpClient();

        // Database
        services.AddDbContext<TamaDbContext>(ConfigureDb);
        services.AddDbContext<ReadOnlyTamaDbContext>(ConfigureDb);

        // Add command, query handlers
        var handlers = typeof(CoreServiceCollectionExtensions)
            .Assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t =>
                t.GetInterfaces()
                    .Any(i =>
                        i.IsGenericType
                        && (
                            i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)
                            || i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)
                        )
                    )
            )
            .ToList();
        foreach (var handler in handlers)
            services.AddScoped(handler);

        return services;

        void ConfigureDb(DbContextOptionsBuilder options) =>
            options.UseSqlite(
                configuration.GetConnectionString("Tama"),
                sqlite => sqlite.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
            );
    }
}

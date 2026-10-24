// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tama.Core.EventHandlers;
using Tama.Core.Events;
using Tama.Core.Features.Commands;
using Tama.Core.Features.Queries;
using Tama.Core.Services;

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

        // Events
        services.AddScoped<IEventBus, InMemoryEventBus>();

        // Services
        services.AddScoped<IAnimeThemesService, AnimeThemesService>();

        // EventHandlers
        services.AddScoped<IEventHandler<EntrySubmittedEvent>, EntrySubmittedEventHandler>();

        return services;

        void ConfigureDb(DbContextOptionsBuilder options) =>
            options.UseSqlite(
                configuration.GetConnectionString("Tama"),
                sqlite => sqlite.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
            );
    }
}

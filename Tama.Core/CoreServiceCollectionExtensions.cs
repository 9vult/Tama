// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        return services;

        void ConfigureDb(DbContextOptionsBuilder options) =>
            options.UseSqlite(
                configuration.GetConnectionString("Tama"),
                sqlite => sqlite.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
            );
    }
}

// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Serilog;
using Tama.Core;
using Tama.Discord;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    Log.Information("Starting Tama host");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog(
        (context, services, config) =>
            config
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
    );

    builder.Services.AddCore(builder.Configuration);
    builder.Services.AddDiscordBot(builder.Configuration);

    var host = builder.Build();

    using (var scope = host.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<TamaDbContext>();
        await db.Database.MigrateAsync();
    }

    using var globalLogScope = host
        .Services.GetRequiredService<ILogger<Program>>()
        .BeginScope(
            new Dictionary<string, object>
            {
                ["CorrelationId"] = "Global",
                ["InteractionType"] = "System",
            }
        );

    // Start everything
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Tama host terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

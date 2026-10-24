// SPDX-License-Identifier: MPL-2.0

using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tama.Core.Events;
using Tama.Discord.Handlers;

namespace Tama.Discord;

public static class DiscordBotServiceCollectionExtensions
{
    public static IServiceCollection AddDiscordBot(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptionsWithValidateOnStart<DiscordOptions>()
            .BindConfiguration(DiscordOptions.Section)
            .ValidateDataAnnotations();

        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton(p =>
        {
            var interactionService = new InteractionService(
                p.GetRequiredService<DiscordSocketClient>()
            );
            return interactionService;
        });

        // Handlers
        services.AddSingleton<InteractionHandler>();

        // EventHandlers
        services.AddScoped<IEventHandler<MatchStartedEvent>, MatchStartedEventHandler>();

        services.AddHostedService<DiscordBotHostedService>();
        return services;
    }
}

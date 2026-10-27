// SPDX-License-Identifier: MPL-2.0

using Discord;
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

        services.AddSingleton(
            new DiscordSocketConfig
            {
                GatewayIntents =
                    GatewayIntents.AllUnprivileged
                    ^ GatewayIntents.GuildScheduledEvents
                    ^ GatewayIntents.GuildInvites,
            }
        );

        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton(p =>
        {
            var interactionService = new InteractionService(
                p.GetRequiredService<DiscordSocketClient>()
            );
            return interactionService;
        });
        services.AddSingleton<MessageUpdateHandler>();

        // Handlers
        services.AddSingleton<InteractionHandler>();

        // EventHandlers
        services.AddScoped<IEventHandler<MatchStartedEvent>, MatchStartedEventHandler>();
        services.AddScoped<IEventHandler<BracketReadyEvent>, BracketReadyEventHandler>();
        services.AddScoped<IEventHandler<BracketCreatedEvent>, BracketCreatedEventHandler>();
        services.AddScoped<IEventHandler<AnnounceChampionEvent>, AnnounceChampionEventHandler>();
        services.AddScoped<IEventHandler<TiebreakRequestedEvent>, TiebreakRequestedEventHandler>();
        services.AddScoped<IEventHandler<EntrySubmittedEvent>, EntrySubmittedEventHandler>();

        services.AddHostedService<DiscordBotHostedService>();
        return services;
    }
}

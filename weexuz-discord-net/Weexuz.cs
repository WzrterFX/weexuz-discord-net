using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Weexuz
{
    public class Weexuz
    {
        static async Task Main()
        {
            IHost host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config.AddYamlFile("Weexuz.yml", optional: false);
                })

                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();

                    logging.SetMinimumLevel(LogLevel.Warning);
                })

                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<DiscordSocketClient>();
                    services.AddSingleton<InteractionService>();

                    services.AddHostedService<Interaction>();
                    services.AddHostedService<Startup>();
                })

                .Build();

            await host.RunAsync();
        }
    }

    public class Interaction : IHostedService
    {
        private readonly DiscordSocketClient _discord;
        private readonly InteractionService _interaction;
        private readonly IServiceProvider _service;
        private readonly ILogger<InteractionService> _logger;

        public Interaction(DiscordSocketClient discord, InteractionService interaction, IServiceProvider service, ILogger<InteractionService> logger)
        {
            _discord = discord;
            _interaction = interaction;
            _service = service;
            _logger = logger;

            _interaction.Log += message => Log(_logger, message);
        }

        public static Task Log(ILogger logger, LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Warning:
                    logger.LogWarning(message.ToString()); break;

                case LogSeverity.Error:
                    logger.LogError(message.ToString()); break;
            }

            return Task.CompletedTask;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _discord.Ready += () => _interaction.RegisterCommandsGloballyAsync(true);
            _discord.InteractionCreated += InteractionRealized;

            await _interaction.AddModulesAsync(Assembly.GetEntryAssembly(), _service);
            _interaction.SlashCommandExecuted += InteractionExecuted;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _interaction.Dispose();

            return Task.CompletedTask;
        }

        private async Task InteractionExecuted(SlashCommandInfo command, IInteractionContext context, IResult result)
        {
            if (!result.IsSuccess && result.Error == InteractionCommandError.UnmetPrecondition)
            {
                SocketGuildUser author = context.User as SocketGuildUser;

                EmbedBuilder embed = new EmbedBuilder()
                    .WithColor(new Color(220, 20, 60))

                    .WithTitle($"**<:weexuz_administration:1227216764396372059> Error something wrong**")

                    .WithFooter($"Requested by {author.Username}", author.GetAvatarUrl());

                await context.Interaction.RespondAsync(embed: embed.Build(), ephemeral: true);
            }
        }

        private async Task InteractionRealized(SocketInteraction interaction)
        {
            try
            {
                SocketInteractionContext context = new SocketInteractionContext(_discord, interaction);
                IResult result = await _interaction.ExecuteCommandAsync(context, _service);

                if (!result.IsSuccess)
                    await context.Channel.SendMessageAsync(result.ToString());
            }
            catch
            {
                if (interaction.Type == InteractionType.ApplicationCommand)
                    await interaction.GetOriginalResponseAsync().ContinueWith(message => message.Result.DeleteAsync());
            }
        }
    }

    public class Startup : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly DiscordSocketClient _discord;
        private readonly ILogger<DiscordSocketClient> _logger;

        public Startup(DiscordSocketClient discord, IConfiguration configuration, ILogger<DiscordSocketClient> logger)
        {
            _discord = discord;
            _configuration = configuration;
            _logger = logger;

            _discord.SetStatusAsync(Enum.Parse<UserStatus>(_configuration["activity:status"]));
            _discord.SetActivityAsync(new Game(_configuration["activity:text"], type: Enum.Parse<ActivityType>(_configuration["activity:type"])));

            _discord.Log += message => Interaction.Log(_logger, message);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _discord.LoginAsync(TokenType.Bot, _configuration["token"]);
            await _discord.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discord.LogoutAsync();
            await _discord.StopAsync();
        }
    }
}
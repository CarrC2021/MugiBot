using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PartyBot.Database;
using PartyBot.Handlers;
using System;
using System.Threading.Tasks;
using Victoria;

namespace PartyBot.Services
{
    public class DiscordService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandHandler _commandHandler;
        private readonly ServiceProvider _services;
        private readonly LavaNode _lavaNode;
        private readonly LavaLinkAudio _audioService;
        private readonly PlayersRulesService _playersRulesService;
        private readonly DataService _dataService;
        private readonly GlobalData _globalData;
        private readonly HelpService _helpService;
        private readonly ReactionService _reactionService;
        private readonly DBManager _dbManager;
        private readonly AMQDBContext _db;
        private readonly AnilistService _anilistService;

        public DiscordService()
        {
            _services = ConfigureServices();
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _commandHandler = _services.GetRequiredService<CommandHandler>();
            _lavaNode = _services.GetRequiredService<LavaNode>();
            _globalData = _services.GetRequiredService<GlobalData>();
            _playersRulesService = _services.GetRequiredService<PlayersRulesService>();
            _db = _services.GetRequiredService<AMQDBContext>();
            _reactionService = new ReactionService();
            _dbManager = new DBManager(_db, _playersRulesService);
            _audioService = _services.GetRequiredService<LavaLinkAudio>();
            _dataService = _services.GetRequiredService<DataService>();
            _helpService = new HelpService(_services.GetRequiredService<CommandService>());
            _anilistService = new AnilistService();
            _dataService.anilistService = _anilistService;

            SubscribeLavaLinkEvents();
            SubscribeDiscordEvents();
        }

        /* Initialize the Discord Client. */
        public async Task InitializeAsync()
        {
            await InitializeGlobalDataAsync();

            await _client.LoginAsync(TokenType.Bot, GlobalData.Config.DiscordToken);
            await _client.StartAsync();

            await _commandHandler.InitializeAsync();
            await Task.Delay(-1);
        }

        /* Hook Any Client Events Up Here. */
        private void SubscribeLavaLinkEvents()
        {
            _lavaNode.OnLog += LogAsync;
            _lavaNode.OnTrackEnded += _audioService.TrackEnded;
        }

        private void SubscribeDiscordEvents()
        {
            _client.Ready += ReadyAsync;
            _client.Log += LogAsync;
            _client.MessageReceived += MessageReceivedAsync;
            _client.ReactionAdded += ReactionAddedAsync;
        }

        private async Task InitializeGlobalDataAsync()
        {
            await _globalData.InitializeAsync();
        }

        /* Used when the Client Fires the ReadyEvent. */
        private async Task ReadyAsync()
        {
            try
            {
                _anilistService.SetEndpoint(GlobalData.Config.LocalEndPoint);
                await _lavaNode.ConnectAsync();
                await _client.SetGameAsync(GlobalData.Config.GameStatus);
            }
            catch (Exception ex)
            {
                await LoggingService.LogInformationAsync(ex.Source, ex.Message);
            }

        }

        /*Used whenever we want to log something to the Console. 
            Todo: Hook in a Custom LoggingService. */
        private async Task LogAsync(LogMessage logMessage)
        {
            await LoggingService.LogAsync(logMessage.Source, logMessage.Severity, logMessage.Message);
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            if (!message.Author.IsBot && message.Channel.Name.Equals("file-uploads") && message.Attachments.Count > 0)
            {
                await _dataService.MessageReceived(message);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        private async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            await _reactionService.ReactionReceieved(cachedMessage, channel, reaction, _dbManager, _audioService);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /* Configure our Services for Dependency Injection. */
        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<LavaNode>()
                .AddSingleton(new LavaConfig())
                .AddSingleton<LavaLinkAudio>()
                .AddSingleton<DataService>()
                .AddSingleton<BotService>()
                .AddSingleton<GlobalData>()
                .AddSingleton<PlayersRulesService>()
                .AddDbContext<AMQDBContext>()
                .AddSingleton<DBManager>()
                .AddSingleton<HelpService>()
                .AddSingleton<ReactionService>()
                .AddSingleton<AMQCircuitService>()
                .AddSingleton<AnilistService>()
                .BuildServiceProvider();
        }
    }
}

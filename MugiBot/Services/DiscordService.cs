﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PartyBot.Database;
using PartyBot.Handlers;
using System;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;

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

            _dbManager.DatabaseAdminIds = GlobalData.Config.DatabaseAdmins;
            _dataService.DBManager.DatabaseAdminIds = GlobalData.Config.DatabaseAdmins;
            await Task.Run(() => SetMainPathValues());
            await SetSubPaths();
            await _commandHandler.InitializeAsync();
            await Task.Delay(-1);
        }

        private void SetMainPathValues()
        {
            _dataService.path = GlobalData.Config.RootFolderPath;
            _playersRulesService.mainpath = GlobalData.Config.RootFolderPath;
            _audioService.path = GlobalData.Config.RootFolderPath;
            _anilistService.path = GlobalData.Config.RootFolderPath;
            _dataService.anilistService.path = GlobalData.Config.RootFolderPath;
            _dataService.DBManager.mainpath = GlobalData.Config.RootFolderPath;
        }
        private async Task SetSubPaths()
        {
            await Task.Run(() => _playersRulesService.SetSubPaths());
            await Task.Run(() => _dataService.DBManager.SetSubPaths());
        }
        /* Hook Any Client Events Up Here. */
        private void SubscribeLavaLinkEvents()
        {
            _lavaNode.OnLog += LogAsync;
            _lavaNode.OnTrackStuck += _audioService.OnTrackStuck;
            // _lavaNode.OnPlayerUpdated += LogPlayerUpdateAsync;
            _lavaNode.OnTrackEnded += _audioService.OnTrackEnded;
        }

        // private async Task LogPlayerUpdateAsync(PlayerUpdateEventArgs args)
        // {
        //     await LoggingService.LogAsync("OnPlayerUpdated", LogSeverity.Verbose, $"Player State:{args.Player.PlayerState}\n Position: {args.Position}\n Track {args.Track}");
        // }

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
            await _reactionService.ReactionReceieved(cachedMessage, channel, reaction, _dataService, _audioService);
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
                .AddSingleton<AnilistService>()
                .BuildServiceProvider();
        }
    }
}

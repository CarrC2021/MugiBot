using System.Linq;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using PartyBot.Database;
using PartyBot.Handlers;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PartyBot.Services
{
    public sealed class DataService
    {
        public DBManager DBManager { get; set; }
        public AnilistService anilistService { get; set; }
        public string path { get; set; }
        private readonly char separator = Path.DirectorySeparatorChar;
        public DataService(DBManager _db)
        {
            path = Path.GetDirectoryName(System.Reflection.
            Assembly.GetExecutingAssembly().GetName().CodeBase).Replace($"{separator}bin{separator}Debug{separator}netcoreapp3.1", "").Replace($"file:{separator}", "");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                path = separator + path;
            DBManager = _db;
        }
        public async Task<Embed> ShowStats(ISocketMessageChannel ch, string show, bool exact = false)
        {
            return await DBCalculationHandler.CalcShowStats(ch, show, exact);
        }

        public async Task<Embed> MergeTest(string mergeFrom, string mergeInto)
        {
            return await DBManager.MergeTest(mergeFrom, mergeInto);
        }
        public async Task<Embed> ListJsons()
        {
            return await JsonHandler.ListJsons(DBManager.JsonFiles);
        }

        public async Task MessageReceived(SocketMessage message)
        {
            await JsonHandler.DownloadJson(message, DBManager.JsonFiles);
        }

        public async Task<Embed> CalcTotalCorrect(PlayersRulesService _service, string rule = "")
        {
            return await DBCalculationHandler.CalcTotalCorrect(_service, rule);
        }

        public async Task<Embed> CalcCorrectByRule(PlayersRulesService _playersRulesService, string rule = "")
        {
            return await DBCalculationHandler.CalcTotalCorrect(_playersRulesService, rule);
        }

        public async Task<Embed> RecommendPracticeSongs(ISocketMessageChannel ch, string name, int numSongs, bool onlyFromList)
        {
            if (numSongs > 30)
                numSongs = 30;
            Dictionary<string, string> players = await DBManager._rs.GetPlayersTracked();
            if (!players.Keys.Contains(name))
                return await EmbedHandler.CreateBasicEmbed("Name Error", "Could not find any players by that name in the database.", Color.Red);
            
            return await DBCalculationHandler.RecommendPracticeSongs(ch, players[name], numSongs, onlyFromList);
        }

        public async Task<Embed> CreatePlaylist(string name)
        {
            if (await PlaylistHandler.CreatePlaylist(name, Path.Combine(path, "playlists", name)))
                return await EmbedHandler.CreateErrorEmbed("Playlist", "Playlist already exists");
            return await EmbedHandler.CreateBasicEmbed("Playlist", $"Playlist {name} now exists", Color.Blue);
        }
        public async Task<Embed> AddToPlaylist(string playlistName, string key)
        {
            if (!await PlaylistHandler.AddToPlaylist(Path.Combine(path, "playlists", playlistName), key))
                return await EmbedHandler.CreateErrorEmbed("Playlist", $"Playlist {playlistName} does not exist");
            return await EmbedHandler.CreateBasicEmbed("Playlist", $"Song has been added to playlist", Color.Blue);
        }
    }
}

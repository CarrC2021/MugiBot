using System.Text;
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
        public DBSearchService _search { get; set; }
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
            if (!await PlaylistHandler.CreatePlaylist(name, Path.Combine(path, "playlists", name.ToLower())))
                return await EmbedHandler.CreateErrorEmbed("Playlist", "Playlist already exists");
            return await EmbedHandler.CreateBasicEmbed("Playlist", $"Playlist {name} now exists", Color.Blue);
        }
        public async Task<Embed> AddToPlaylist(string playlistName, string key)
        {
            if (DBSearchService.UseSongKey(key) == null)
                return await EmbedHandler.CreateErrorEmbed("Playlist", $"Song key {key} is invalid");
            if (!await PlaylistHandler.AddToPlaylist(Path.Combine(path, "playlists", playlistName), key))
                return await EmbedHandler.CreateErrorEmbed("Playlist", $"Playlist {playlistName} does not exist");
            return await EmbedHandler.CreateBasicEmbed("Playlist", $"Song has been added to playlist {key}", Color.Blue);
        }

        public async Task<Embed> ShufflePlaylist(string playlistName)
        {
            if (!File.Exists(Path.Combine(path, "playlists", playlistName)))
                return await EmbedHandler.CreateErrorEmbed("Playlist", $"Playlist {playlistName} does not exist");
            await PlaylistHandler.ShufflePlaylist(Path.Combine(path, "playlists", playlistName));
            return await EmbedHandler.CreateBasicEmbed("Playlist", $"{playlistName} has been shuffled", Color.Blue);
        }

        public async Task<Embed> RemoveFromPlaylist(string playlistName, string key)
        {
            if (!File.Exists(Path.Combine(path, "playlists", playlistName.ToLower())))
                return await EmbedHandler.CreateErrorEmbed("Playlist", $"Playlist {playlistName.ToLower()} does not exist");
            await PlaylistHandler.RemoveFromPlaylist(Path.Combine(path, "playlists", playlistName.ToLower()), key);
            return await EmbedHandler.CreateBasicEmbed("Playlist", $"{key} has been removed from {playlistName}", Color.Blue);
        }
        public async Task<Embed> PrintPlaylist(string playlistName, ISocketMessageChannel channel)
        {
            var filePath = Path.Combine(path, "playlists", playlistName.ToLower());
            var filePath2 = Path.Combine(path, "playlists", "artists", playlistName.ToLower());
            var filePath3 = Path.Combine(path, "playlists", "shows", playlistName.ToLower());

            string toUse;
            if (File.Exists(filePath3))
                toUse = filePath3;
            else if (File.Exists(filePath2))
                toUse = filePath2;
            else if (File.Exists(filePath))
                toUse = filePath;
            else
                return await EmbedHandler.CreateErrorEmbed("Playlist", $"Playlist {playlistName.ToLower()} does not exist");
            var embeds = new List<Embed>();

            var content = await PlaylistHandler.LoadPlaylist(toUse);

            var sb = new StringBuilder();
            sb.Append($"{playlistName} songs: \n\n");
            foreach (string key in content)
            {
                var tableObject = await DBSearchService.UseSongKey(key);
                if (sb.Length + SongTableObject.PrintSong(tableObject).Length > 2000)
                {
                    embeds.Add(await EmbedHandler.CreateBasicEmbed("Playlist", sb.ToString(), Color.Blue));
                    sb.Clear();
                    sb.Append($"{playlistName} songs: \n\n");
                }
                sb.Append(SongTableObject.PrintSong(tableObject) + "\n\n");
            }
            embeds.Add(await EmbedHandler.CreateBasicEmbed("Playlist", sb.ToString(), Color.Blue));
            foreach (Embed embed in embeds)
            {
                await channel.SendMessageAsync(embed: embed);
            }
            return await EmbedHandler.CreateBasicEmbed("Playlists", $"A total of {content.Count} songs are in {playlistName}. To shuffle the order "
                + "of the songs use the !shuffleplaylist command.", Color.Blue);
        }
    }
}

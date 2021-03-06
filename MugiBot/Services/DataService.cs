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
using PartyBot.DataStructs;

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

        public async Task<Embed> DeleteJson(string fileName)
        {
            return await JsonHandler.DeleteJson(DBManager.JsonFiles, fileName);
        }

        public async Task MessageReceived(SocketMessage message)
        {
            await JsonHandler.DownloadJson(message, DBManager.JsonFiles);
            await PlaylistHandler.DownloadPlaylistFile(message, Path.Combine(path, "PlaylistDownloads"));
        }

        public async Task<Embed> CalcTotalCorrect(PlayersRulesService _service, string rule = "")
        {
            return await DBCalculationHandler.CalcTotalCorrect(_service, rule);
        }

        public async Task<Embed> CalcCorrectByRule(PlayersRulesService _playersRulesService, string rule = "")
        {
            return await DBCalculationHandler.CalcTotalCorrect(_playersRulesService, rule);
        }

        public async Task<Embed> PrintAllLists()
        {
            var malLists = Directory.GetFiles(Path.Combine(GlobalData.Config.RootFolderPath, "MALUserLists")).Select(f => Path.GetFileName(f));
            var anilists = Directory.EnumerateFiles(Path.Combine(GlobalData.Config.RootFolderPath, "AniLists")).Select(f => Path.GetFileName(f));
            var sb = new StringBuilder();
            sb.Append("All lists:\n\n My Anime Lists:\n");
            foreach (string name in malLists)
                sb.Append($"{name.Replace(".json","")}\n");
            sb.Append("\nAnilists:\n");
            foreach (string name in anilists)
                sb.Append($"{name.Replace(".json","")}\n");
            return await EmbedHandler.CreateBasicEmbed("User Lists", sb.ToString(), Color.Blue);
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

        public async Task<Embed> CreatePrivatePlaylist(string name, ulong ID)
        {
            var creatorName = ID.ToString();
            if (!await PlaylistHandler.CreatePrivatePlaylist(Path.Combine(path, "playlists", name.ToLower()), creatorName))
                return await EmbedHandler.CreateErrorEmbed("Playlist", "Playlist already exists");
            return await EmbedHandler.CreateBasicEmbed("Playlist", $"A private playlist with the name {name} now exists", Color.Blue);
        }

        public async Task<Embed> AddToPlaylist(string playlistName, string key, ulong author = 10)
        {
            if (DBSearchService.UseSongKey(key) == null)
                return await EmbedHandler.CreateErrorEmbed("Playlist", $"Song key {key} is invalid");
            var tuple = await PlaylistHandler.AddToPlaylist(Path.Combine(path, "playlists", playlistName), key, author);
            if (!tuple.Item1)
                return await EmbedHandler.CreateErrorEmbed("Playlist", $"{tuple.Item2}");
            return await EmbedHandler.CreateBasicEmbed("Playlist", $"The song {tuple.Item3.PrintSong()} has been added to the playlist {playlistName}", Color.Blue);
        }
        public async Task<Embed> RemoveFromPlaylist(string playlistName, string key, ulong author = 1)
        {
            if (!File.Exists(Path.Combine(path, "playlists", playlistName.ToLower())))
                return await EmbedHandler.CreateErrorEmbed("Playlist", $"Playlist {playlistName.ToLower()} does not exist");
            var tuple = await PlaylistHandler.RemoveFromPlaylist(Path.Combine(path, "playlists", playlistName.ToLower()), key, author);
            if (!tuple.Item1)
                return await EmbedHandler.CreateErrorEmbed("Playlist", $"{tuple.Item2}");
            SongTableObject songObject = await DBSearchService.UseSongKey(key);
            return await EmbedHandler.CreateBasicEmbed("Playlist", $"{songObject.PrintSong()} has been removed from {playlistName}", Color.Blue);
        }
        public async Task<Embed> CreatePlaylistFromGameData(string name, ulong id, SocketUserMessage message)
        {
            if (message.Attachments.Count == 0)
                return await EmbedHandler.CreateErrorEmbed("Playlists", "You did not attach a file. You need to do that to use this method.");
            if (File.Exists(Path.Combine(path, "playlists", name)))
                return await EmbedHandler.CreateErrorEmbed("Playlists", "A playlist with this name already exists, use a different name.");
            var songs = new List<SongTableObject>(); 
            await JsonHandler.DownloadJson(message, Path.Combine(path, "jsonsforplaylist"), false);
            await JsonHandler.DownloadJson(message, Path.Combine(path, DBManager.JsonFiles));
            await DBManager.AddAllToDatabase();
            var finalList = new List<SongData>();
            foreach (string fileName in Directory.EnumerateFiles(Path.Combine(path, "jsonsforplaylist")))
            {
                var dataList = await JsonHandler.ConvertJsonToSongData(new FileInfo(fileName));
                finalList.AddRange(dataList);
            }
            return await PlaylistHandler.PlaylistFromGameData(await DBManager._rs.GetPlayersTracked(), finalList, id,
            DBManager.mainpath, name);
        }

        public async Task<Embed> PrintPlaylist(string playlistName, ISocketMessageChannel channel)
        {
            string fileName = Path.Combine(path, "playlists", playlistName);
            if (fileName == null)
                return await EmbedHandler.CreateErrorEmbed("Playlist does not exist", $"{playlistName} does not exist");
            var embeds = new List<Embed>();

            var content = await PlaylistHandler.LoadPlaylist(fileName);

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
            return await EmbedHandler.CreateBasicEmbed("Playlists", $"A total of {content.Count} songs are in {playlistName}.", Color.Blue);
        }
    }
}

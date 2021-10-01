using System;
using System.Linq;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using System.IO;
using PartyBot.DataStructs;
using PartyBot.Handlers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PartyBot.Database;
using System.Text;

namespace PartyBot.Handlers
{
    public static class PlaylistHandler
    {
        public static async Task<bool> CreatePlaylist(string name, string filePath)
        {
            if (File.Exists(filePath))
                return false;
            await Task.Run(() => File.Create(filePath));
            await Task.Run(() => JsonConvert.SerializeObject(new Playlist("public", new Dictionary<string, string>())));
            return true;
        }
        public static async Task<bool> CreatePrivatePlaylist(string name, string filePath, string playlistCreator)
        {
            if (File.Exists(filePath))
                return false;
            await Task.Run(() => File.Create(filePath));
            await Task.Run(() => JsonConvert.SerializeObject(new Playlist(playlistCreator, new Dictionary<string, string>())));
            return true;
        }
        public static async Task DeletePlaylist(string name, string filePath)
        {
            if (File.Exists(filePath))
                await Task.Run(() => File.Delete(filePath));
        }
        public static async Task<bool> AddMultipleToPlaylist(string filePath, List<string> songkeys)
        {
            if (File.Exists(filePath))
            {
                Playlist contents = await Task.Run(() => JsonConvert.DeserializeObject<Playlist>(filePath));
                foreach (string key in songkeys)
                {
                    if (!contents.Songs.ContainsKey(key))
                    {
                        var songObject = await DBSearchService.UseSongKey(key);
                        contents.Songs.Add(key, SongTableObject.PrintSong(songObject));
                    }
                }
                await SerializeAndWrite(contents, filePath);
                return true;
            }
            return false;
        }
        public static async Task<bool> AddToPlaylist(string filePath, string songkey)
        {
            if (File.Exists(filePath))
            {
                Playlist contents = await Task.Run(() => JsonConvert.DeserializeObject<Playlist>(filePath));
                if (!contents.Songs.ContainsKey(songkey))
                {
                    var songObject = await DBSearchService.UseSongKey(songkey);
                    contents.Songs.Add(songkey, SongTableObject.PrintSong(songObject));
                }
                await SerializeAndWrite(contents, filePath);
                return true;
            }
            return false;
        }
        public static async Task<bool> RemoveFromPlaylist(string filePath, string songkey)
        {
            if (File.Exists(filePath))
            {
                Playlist contents = await Task.Run(() => JsonConvert.DeserializeObject<Playlist>(filePath));
                if (contents.Songs.ContainsKey(songkey))
                    contents.Songs.Remove(songkey);

                await SerializeAndWrite(contents, filePath);
                return true;
            }
            return false;
        }
        public static async Task<List<string>> LoadPlaylist(string filePath)
        {
            Playlist playlist;
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var content = await File.ReadAllTextAsync(filePath);
            playlist = await Task.Run(() => JsonConvert.DeserializeObject<Playlist>(content, settings));
            return playlist.Songs.Keys.ToList();
        }
        public static async Task<Dictionary<string, string>> ReturnPlaylistDictionary(string filePath)
        {
            var content = await File.ReadAllTextAsync(filePath);
            Playlist playlist = await Task.Run(() => JsonConvert.DeserializeObject<Playlist>(content));
            return playlist.Songs;
        }
        public static async Task ShufflePlaylist(string filePath)
        {
            Playlist playlist;
            var content = await File.ReadAllTextAsync(filePath);
            playlist = await Task.Run(() => JsonConvert.DeserializeObject<Playlist>(content));
            var rnd = new Random();
            playlist.Songs.OrderBy(item => rnd.Next()).ToDictionary(item => item.Key, item => item.Value);
            await SerializeAndWrite(playlist, filePath);
        }
        public static async Task SerializeAndWrite(Playlist content, string filePath)
        {
            var jsonObject = JsonConvert.SerializeObject(content);
            await File.WriteAllTextAsync(filePath, jsonObject);
        }
        public static async Task<Embed> UpdatePlaylists(string directoryName)
        {
            foreach (string file in Directory.EnumerateFiles(directoryName))
            {
                await UpdatePlaylist(file);
            }
            return await EmbedHandler.CreateBasicEmbed("Playlists", "All the playlists should be updated", Color.Blue);
        }
        public static async Task UpdatePlaylist(string fileName)
        {
            var contents = await File.ReadAllLinesAsync(fileName);
            Playlist newContents = new Playlist("public", new Dictionary<string, string>());
            var filtered = contents.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            foreach (string line in filtered)
            {
                var songObject = await DBSearchService.UseSongKey(line);
                if (songObject != null && !newContents.Songs.ContainsKey(songObject.Key))
                    newContents.Songs.Add(line, SongTableObject.PrintSong(songObject));
            }
            await SerializeAndWrite(newContents, fileName);
        }

        public static string SearchPlaylistDirectories(string path, string query)
        {
            var list = new List<string>();
            if (File.Exists(Path.Combine(path, "artists", query)))
                list.Append(Path.Combine(path, "artists", query));
                
            if (File.Exists(Path.Combine(path, "shows", query)))
                list.Append(Path.Combine(path, "shows", query));
            
            if (File.Exists(Path.Combine(path, query)))
                list.Append(Path.Combine(path, query));

            return "Figure out what to do with this later";
        }

        public static async Task<Embed> CreateArtistPlaylist(string artistName, string artistPlaylistDirectory, bool exact = false)
        {
            return await AutomaticPlaylistCreation(artistName, artistPlaylistDirectory, "artist", "any", true);
        }

        public static async Task<Embed> CreateShowPlaylist(string show, string showPlaylistDirectory, string songType = "any", bool exact = false)
        {
            return await AutomaticPlaylistCreation(show, showPlaylistDirectory, "show", songType, exact);
        }

        public static async Task<Embed> AutomaticPlaylistCreation(string query, string playlistDirectory, string searchType, string songType = "any", bool exact = false)
        {
            if (File.Exists(Path.Combine(playlistDirectory, query.ToLower())))
                return await EmbedHandler.CreateErrorEmbed("Playlists", $"A playlist with name {query.ToLower()} already exists");
            if (File.Exists(Path.Combine(playlistDirectory, songType, query.ToLower())))
                return await EmbedHandler.CreateErrorEmbed("Playlists", $"A playlist with name {query.ToLower()} already exists");
            var songs = new List<SongTableObject>();
            Console.WriteLine(songType);
            if (searchType.Equals("artist"))
                songs = await DBSearchService.ReturnSongsByAuthor(query);
            if (searchType.Equals("show"))
                songs = await DBSearchService.ReturnAllSongObjectsByShowByType(query, songType, exact);
            if (songs.Count == 0)
                return await EmbedHandler.CreateBasicEmbed("Playlists", $"A playlist with name {query.ToLower()} not created. "
            + "The query you specified returned 0 songs. Blame Dayt not me :wink:.", Color.Red);
            var playlist = new Playlist("public", new Dictionary<string, string>(), true, false);
            playlist.AutomaticallyGenerated = true;
            // Now we populate the dictionary with our songs we found.
            foreach (SongTableObject song in songs)
                playlist.Songs.Add(song.Key, SongTableObject.PrintSong(song));

            // Once the dictionary has been populated we serialize and write the json to a file.
            if (!songType.Equals("any"))
            {
                songType += "s";
                await SerializeAndWrite(playlist, Path.Combine(playlistDirectory, $"{query.ToLower()} {songType.ToLower()}"));
            }
            else
            {
                await SerializeAndWrite(playlist, Path.Combine(playlistDirectory, query.ToLower()));
                songType = "";
            }
            return await EmbedHandler.CreateBasicEmbed("Playlists", $"A playlist with name {query.ToLower()} {songType.ToLower()} now exists. "
            + "It will contain any song in the database by the query you specified.", Color.Blue);
        }

        // This function will return all of the names of the files in the specified playlist directory.
        public static async Task<Embed> ListPlaylistsInDirectory(string directoryName, ISocketMessageChannel channel)
        {
            var sb = new StringBuilder();
            var fileNames = Directory
                    .GetFiles(directoryName, "*", SearchOption.AllDirectories)
                    .Select(f => Path.GetFileName(f));
            foreach (string file in fileNames)
            {
                if (sb.Length + file.Length > 2000)
                {
                    await channel.SendMessageAsync(embed: await EmbedHandler.CreateBasicEmbed("Playlists", sb.ToString(), Color.Blue));
                    sb.Clear();
                }
                sb.Append(file);
            }
            return await EmbedHandler.CreateBasicEmbed("Playlists", sb.ToString(), Color.Blue);
        }
    }
}
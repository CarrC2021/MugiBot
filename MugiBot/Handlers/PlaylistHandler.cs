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
using System.Net;

namespace PartyBot.Handlers
{
    public static class PlaylistHandler
    {
        public static async Task<bool> CreatePlaylist(string name, string filePath, Dictionary<string, string> dict = null)
        {
            if (File.Exists(filePath))
                return false;
            if (dict == null)
                dict = new Dictionary<string, string>();
            await SerializeAndWrite(new Playlist("public", dict), filePath);
            return true;
        }
        public static async Task DownloadPlaylistFile(SocketMessage message, string filePath)
        {
            string fileName = "";
            try
            {
                for (int i = 0; i < message.Attachments.Count; i++)
                {
                    fileName = message.Attachments.ElementAt(i).Filename;
                    if (fileName.EndsWith("txt"))
                    {
                        //Create a WebClient and download the attached file
                        using var client = new WebClient();
                        var bytes = await client.DownloadDataTaskAsync(new Uri(message.Attachments.ElementAt(i).Url));
                        await File.WriteAllBytesAsync(Path.Combine(filePath, fileName), bytes);
                        client.Dispose();
                        await message.Channel.SendMessageAsync(embed: await EmbedHandler.CreateBasicEmbed("File Downloads", $"Downloaded a file named {fileName}", Color.Blue));
                        await message.Channel.SendMessageAsync(embed: await PlaylistHandler.CreatePlaylistFromFile(filePath, fileName, message.Author.Id));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex.Source);
            }
            Console.WriteLine("Downloads: Does this look like the file you wanted to download? " + fileName);
        }
        public static async Task<Embed> CreatePlaylistFromFile(string path, string fileName, ulong ID)
        {
            var filePath = Path.Combine(path, fileName);
            if (!File.Exists(filePath))
                return await EmbedHandler.CreateErrorEmbed("Playlist", "Could not find a file with that name");
            var lines = await File.ReadAllLinesAsync(filePath);
            var dict = new Dictionary<string, string>();
            foreach (string line in lines)
            {
                var song = await DBSearchService.UseSongKey(line);
                if (song == null)
                {
                    File.Delete(filePath);
                    return await EmbedHandler.CreateErrorEmbed("Playlist", $"Invalid song key: {line}, fix this line and reupload.");
                }
                dict.TryAdd(song.Key, song.PrintSong());
            }
            var creatorName = ID.ToString();
            if (!await PlaylistHandler.CreatePrivatePlaylist(Path.Combine(path.Replace($"PlaylistDownloads", ""), "playlists", fileName.ToLower().Replace(".txt", "")), creatorName, dict))
                return await EmbedHandler.CreateErrorEmbed("Playlist", "Playlist already exists");
            await Task.Run(() => File.Delete(filePath));
            return await EmbedHandler.CreateBasicEmbed("Playlist", $"Created a playlist with the name {fileName}", Color.Blue);
        }
        public static async Task<bool> CreatePrivatePlaylist(string filePath, string playlistCreator, Dictionary<string, string> dict = null)
        {
            if (File.Exists(filePath))
                return false;
            if (dict == null)
                dict = new Dictionary<string, string>();
            await SerializeAndWrite(new Playlist(playlistCreator, dict, true), filePath);
            return true;
        }
        // Given a file name this function will deserialize a json file asynchronously and return it as a Playlist object
        public static async Task<Playlist> DeserializePlaylistAsync(string filePath)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var content = await File.ReadAllTextAsync(filePath);
            Playlist contents = await Task.Run(() => JsonConvert.DeserializeObject<Playlist>(content, settings));
            return contents;
        }
        public static async Task<Embed> DeletePlaylist(string name, string filePath, string author = "public")
        {
            if (!File.Exists(filePath))
                return await EmbedHandler.CreateBasicEmbed("Playlists", $"There is no playlist with the name {name}", Color.Blue);
            var playlist = await DeserializePlaylistAsync(filePath);
            if (playlist.Private && playlist.Author.Equals(author))
            {
                await Task.Run(() => File.Delete(filePath));
                return await EmbedHandler.CreateBasicEmbed("Playlists", $"Deleted the playlist {name}", Color.Blue);
            }
            if (playlist.Private || playlist.AutomaticallyGenerated)
                return await EmbedHandler.CreateErrorEmbed("Playlists", $"You do not have the permission to delete the playlist {name}");
            await Task.Run(() => File.Delete(filePath));
            return await EmbedHandler.CreateBasicEmbed("Playlists", $"Deleted the playlist {name}", Color.Blue);
        }
        public static async Task<bool> AddMultipleToPlaylist(string filePath, List<string> songkeys)
        {
            if (!File.Exists(filePath))
                return false;

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
        public static async Task<Tuple<bool, string>> AddToPlaylist(string filePath, string songkey, ulong author = 1)
        {
            if (File.Exists(filePath))
            {
                var contents = await DeserializePlaylistAsync(filePath);
                if (contents.Private && !contents.Author.Equals(author))
                    return new Tuple<bool, string>(false, "You do not have permission to add to this playlist.");
                if (!contents.Songs.ContainsKey(songkey))
                {
                    var songObject = await DBSearchService.UseSongKey(songkey);
                    contents.Songs.Add(songkey, SongTableObject.PrintSong(songObject));
                }
                await SerializeAndWrite(contents, filePath);
                return new Tuple<bool, string>(true, "");
            }
            return new Tuple<bool, string>(false, "This playlist does not exist");
        }
        public static async Task<Tuple<bool, string>> RemoveFromPlaylist(string filePath, string songkey, ulong author = 1)
        {
            if (File.Exists(filePath))
            {
                var contents = await DeserializePlaylistAsync(filePath);
                if (contents.Private && !contents.Author.Equals(author))
                    return new Tuple<bool, string>(true, "You do not have permission to add to this playlist.");
                if (contents.Songs.ContainsKey(songkey))
                    contents.Songs.Remove(songkey);

                await SerializeAndWrite(contents, filePath);
                return new Tuple<bool, string>(true, "");
            }
            return new Tuple<bool, string>(false, "");
        }

        public static async Task<List<string>> LoadPlaylist(string filePath)
        {
            var playlist = await DeserializePlaylistAsync(filePath);
            return playlist.Songs.Keys.ToList();
        }
        public static async Task<Dictionary<string, string>> ReturnPlaylistDictionary(string filePath)
        {
            var content = await File.ReadAllTextAsync(filePath);
            Playlist playlist = await Task.Run(() => JsonConvert.DeserializeObject<Playlist>(content));
            return playlist.Songs;
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
                list.Add(Path.Combine(path, "artists", query));

            if (File.Exists(Path.Combine(path, "shows", query)))
                list.Add(Path.Combine(path, "shows", query));

            if (File.Exists(Path.Combine(path, query)))
                list.Add(Path.Combine(path, query));

            if (list.Count == 0)
                return null;
            return list.FirstOrDefault();
        }

        // This function should be moved into DBSearchService or into searchhandler
        public static async Task<List<SongTableObject>> LoadSongsForQuery(string query, string searchType, string songType = "any", bool exact = false)
        {
            var songs = new List<SongTableObject>();
            Console.WriteLine(songType);
            // Use the correct search command.
            if (searchType.Equals("artist"))
                songs = await DBSearchService.ReturnSongsByAuthor(query, exact);
            if (searchType.Equals("show"))
                songs = await SearchHandler.ShowSearch(query, songType, exact);
            return songs;
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
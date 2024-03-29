using System;
using System.Linq;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using System.IO;
using PartyBot.DataStructs;
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
            await SerializeAndWrite(new Playlist(500, dict), filePath);
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
                        await message.Channel.SendMessageAsync(embed: await PlaylistHandler.AddToPlaylistFromFile(filePath, fileName, message.Author.Id));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex.Source);
            }
            Console.WriteLine("Downloads: Does this look like the file you wanted to download? " + fileName);
        }
        public static async Task<Embed> AddToPlaylistFromFile(string path, string fileName, ulong ID)
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
            var creatorName = ID;
            var playlistName = Path.Combine(path.Replace($"PlaylistDownloads", ""), "playlists", fileName.ToLower().Replace(".txt", ""));
            // If no one has created a playlist with that name we use the create playlist function.
            if (!File.Exists(playlistName))
            {
                await PlaylistHandler.CreatePrivatePlaylist(playlistName, creatorName, dict);
                await Task.Run(() => File.Delete(filePath));
                return await EmbedHandler.CreateBasicEmbed("Playlist", $"Created a playlist with the name {fileName}", Color.Blue);
            }
            // If someone has already made a playlist with that name, and this person is the owner of the playlist then we update with the new songs
            Playlist playlistContents = await DeserializePlaylistAsync(fileName);
            if (creatorName == playlistContents.Author)
            {
                await AddMultipleToPlaylist(fileName, playlistContents.Songs.Keys.ToList());
                await Task.Run(() => File.Delete(filePath));
                return await EmbedHandler.CreateBasicEmbed("Playlist", $"Added songs to the playlist with the name {fileName}", Color.Blue);
            }
            return await EmbedHandler.CreateErrorEmbed("Playlist", $"A playlist exists with the name {fileName} but you are not the author of that file.");
        }
        public static async Task<bool> CreatePrivatePlaylist(string filePath, ulong playlistCreator, Dictionary<string, string> dict = null)
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
                    if (songObject != null)
                        contents.Songs.Add(key, SongTableObject.PrintSong(songObject));
                }
            }
            await SerializeAndWrite(contents, filePath);
            return true;

        }
        public static async Task<Tuple<bool, string, SongTableObject>> AddToPlaylist(string filePath, string songkey, ulong author = 1)
        {
            if (File.Exists(filePath))
            {
                var contents = await DeserializePlaylistAsync(filePath);
                if (contents.Private && !contents.Author.Equals(author))
                    return new Tuple<bool, string, SongTableObject>(false, "You do not have permission to add to this playlist.", null);
                if (!contents.Songs.ContainsKey(songkey))
                {
                    var songObject = await DBSearchService.UseSongKey(songkey);
                    contents.Songs.Add(songkey, SongTableObject.PrintSong(songObject));
                    await SerializeAndWrite(contents, filePath);
                    return new Tuple<bool, string, SongTableObject>(true, "", songObject);
                }
            }
            return new Tuple<bool, string, SongTableObject>(false, "This playlist does not exist", null);
        }
        public static async Task<Tuple<bool, string>> RemoveFromPlaylist(string filePath, string songkey, ulong author = 1)
        {
            if (!File.Exists(filePath))
                return new Tuple<bool, string>(true, "");
                var contents = await DeserializePlaylistAsync(filePath);
            if (contents.Private && !contents.Author.Equals(author))
                return new Tuple<bool, string>(true, "You do not have permission to add to this playlist.");
            if (!contents.Songs.ContainsKey(songkey))
                return new Tuple<bool, string>(true, "");
            contents.Songs.Remove(songkey);
            await SerializeAndWrite(contents, filePath);
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

        // This function should be moved into DBSearchService or into searchhandler
        public static async Task<List<SongTableObject>> LoadSongsForQuery(string query, string searchType, string songType = "any", bool exact = false)
        {
            var songs = new List<SongTableObject>();
            Console.WriteLine(songType);
            // Use the correct search command.
            if (searchType.Equals("artist"))
                songs = await DBSearchService.ReturnSongsByArtist(query, exact);
            if (searchType.Equals("show"))
                songs = await SearchHandler.ShowSearch(query, songType, exact);
            return songs;
        }

        public static async Task<Embed> PrintAllPlaylists(string path, ISocketMessageChannel channel) 
        {
            char separator = Path.DirectorySeparatorChar;
            var fileNames = Directory.EnumerateFiles(Path.Combine(path, "playlists"));
            var sortedFiles = new List<string>();
            var sb = new StringBuilder();
            sb.Append("Playlist Names: \n\n");
            foreach (var file in fileNames)
            {

                List<string> fileName = file.Split(separator).ToList();
                sortedFiles.Add(fileName[fileName.Count - 1]);
            }
            sortedFiles.Sort();
            foreach (var name in sortedFiles)
            {
                if (2048 <= sb.Length+ $"{name}\n\n".Length)
                {
                    await channel.SendMessageAsync(embed: await EmbedHandler.CreateBasicEmbed("Playlists", sb.ToString(), Color.Blue));
                    sb.Clear();
                    sb.Append("Playlist Names: \n\n");
                }
                sb.Append($"{name}\n\n");
            }
            return await EmbedHandler.CreateBasicEmbed("Playlists", sb.ToString(), Color.Blue);
        }

        public static async Task<Embed> PlaylistFromGameData(Dictionary<string, string> PlayerDict, List<SongData> songs, ulong id, string path, string fileName)
        {
            using var db = new AMQDBContext();
            var user = await db.DiscordUsers.FindAsync(id);
            if (user == null || user.DatabaseName == null)
                return await EmbedHandler.CreateErrorEmbed("Playlist Creation", "You have not set your database information. To do so use the !setdbusername command.");
            try
            {
                var newPlaylist = new Playlist();
                Console.WriteLine("Got to the playlist creation step.");
                newPlaylist.Author = id;
                Console.WriteLine(newPlaylist.Author);
                newPlaylist.AutomaticallyGenerated = true;
                foreach (SongData song in songs)
                {
                    foreach (Player player in song.players)
                    {
                        // Only add songs that the player heard and got wrong
                        if (!PlayerDict.ContainsKey(player.name) || PlayerDict[player.name] != user.DatabaseName || player.correct)
                            continue;
                        var tempObject = await db.SongTableObject.FindAsync(song.MakeSongTableKey());
                        newPlaylist.Songs.Add(tempObject.Key, tempObject.PrintSong());
                    }
                }
                await SerializeAndWrite(newPlaylist, Path.Combine(path, "playlists", fileName));
                return await EmbedHandler.CreateBasicEmbed("Playlists", $"Created a playlist named {fileName} where the songs are the songs you missed in the files provided.", Color.Blue);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return await EmbedHandler.CreateErrorEmbed("Playlists", "Something went wrong while trying to create the new playlist.");
            }
        }
    }
}
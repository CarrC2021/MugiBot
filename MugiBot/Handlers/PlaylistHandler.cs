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
            Playlist contents;
            contents = await Task.Run(() => JsonConvert.DeserializeObject<Playlist>(filePath));
            return contents.Songs.Keys.ToList();
        }
        public static async Task<Dictionary<string, string>> ReturnPlaylistDictionary(string filePath)
        {
            Playlist contents;
            contents = await Task.Run(() => JsonConvert.DeserializeObject<Playlist>(filePath));
            return contents.Songs;
        }
        public static async Task ShufflePlaylist(string filePath)
        {
            Playlist contents;
            contents = await Task.Run(() => JsonConvert.DeserializeObject<Playlist>(filePath));
            var rnd = new Random();
            contents.Songs.OrderBy(item => rnd.Next()).ToDictionary(item => item.Key, item => item.Value);
            await SerializeAndWrite(contents, filePath);
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
                newContents.Songs.Add(line, SongTableObject.PrintSong(songObject));
            }
        }
        public static async Task<Embed> CreateArtistPlaylist(string artistName, string artistPlaylistDirectory)
        {
            if (File.Exists(Path.Combine(artistPlaylistDirectory, artistName)))
                return await EmbedHandler.CreateErrorEmbed("Playlists", $"An artist playlist with name {artistName} already exists");
            var songs = await DBSearchService.ReturnSongsByAuthor(artistName);
            var playlist = new Playlist("public", new Dictionary<string, string>());
            
            // Now we populate the dictionary with our songs we found.
            foreach (SongTableObject song in songs)
                playlist.Songs.Add(song.Key, SongTableObject.PrintSong(song));

            // Once the dictionary has been populated we serialize and write the json to a file.
            await SerializeAndWrite(playlist, Path.Combine(artistPlaylistDirectory, artistName));
            return await EmbedHandler.CreateBasicEmbed("Playlists", $"An artist playlist with name {artistName} now exists. "
            + "It will contain any song in the database by the artist you specified.", Color.Blue);
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
using System;
using System.Linq;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using System.IO;
using PartyBot.Database;
using PartyBot.Handlers;
using System.Threading.Tasks;

namespace PartyBot.Handlers
{
    public static class PlaylistHandler
    {
        public static async Task<bool> CreatePlaylist(string name, string filePath)
        {
            if (File.Exists(filePath))
                return false;
            await Task.Run(() => File.Create(filePath));
            return true;
        }
        public static async Task DeletePlaylist(string name, string filePath)
        {
            if (File.Exists(filePath))
                await Task.Run(() => File.Delete(filePath));
        }
        public static async Task<bool> AddToPlaylist(string filePath, List<string> songkeys)
        {
            if (File.Exists(filePath))
            {
                var contents = await LoadPlaylist(filePath);
                foreach (string key in songkeys)
                {
                    if (!contents.Contains(key))
                    {
                        await File.AppendAllTextAsync(filePath, key + "\n");
                    }
                }
                return true;
            }
            return false;
        }
        public static async Task<bool> AddToPlaylist(string filePath, string songkey)
        {
            if (File.Exists(filePath))
            {
                var contents = await LoadPlaylist(filePath);
                if (!contents.Contains(songkey))
                {
                    await File.AppendAllTextAsync(filePath, songkey + "\n");
                    return true;
                }
            }
            return false;
        }
        public static async Task<bool> RemoveFromPlaylist(string filePath, string songkey)
        {
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.WriteAllLines(filePath,
                    File.ReadLines(filePath).Where(l => l != songkey).ToList()));
                return true;
            }
            return false;
        }
        public static async Task<List<string>> LoadPlaylist(string filePath)
        {
            var contents = await File.ReadAllLinesAsync(filePath);
            return contents.ToList();
        }
        public static async Task ShufflePlaylist(string filePath)
        {
            var list = await LoadPlaylist(filePath);
            var rnd = new Random();
            var randomizedList = list.OrderBy(item => rnd.Next());
            await File.WriteAllLinesAsync(filePath, randomizedList);
        }
    }
}
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
                return false;
            foreach (string key in songkeys)
                await File.AppendAllTextAsync(filePath, key + "\n");
            return true;
        }
        public static async Task<bool> AddToPlaylist(string filePath, string songkey)
        {
            if (File.Exists(filePath))
                return false;
            await File.AppendAllTextAsync(filePath, songkey + "\n");
            return true;
        }
        public static async Task<bool> RemoveFromPlaylist(string filePath, string songkey)
        {
            if (File.Exists(filePath))
                return false;
            await Task.Run(() => File.WriteAllLines(filePath,
               File.ReadLines(filePath).Where(l => l != songkey).ToList()));
            return true;
        }
        public static async Task<List<string>> LoadPlaylist(string filePath)
        {
            var contents = await File.ReadAllLinesAsync(filePath);
            return contents.ToList();
        }
    }
}
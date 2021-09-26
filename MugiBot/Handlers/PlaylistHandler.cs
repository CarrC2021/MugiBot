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
        public static async Task AddToPlaylist(string filepath, List<string> songkeys)
        {
            foreach (string key in songkeys)
                await File.AppendAllTextAsync(filepath, key + "\n");
        }
        public static async Task AddToPlaylist(string filePath, string songkey)
        {
            await File.AppendAllTextAsync(filePath, songkey + "\n");
        }
        public static async Task RemoveFromPlaylist(string filePath, string songkey)
        {
            await Task.Run(() => File.WriteAllLines(filePath,
               File.ReadLines(filePath).Where(l => l != songkey).ToList()));
        }
        public static async Task<List<string>> LoadPlaylist(string filePath)
        {
            var contents = await File.ReadAllLinesAsync(filePath);
            return contents.ToList();
        }
    }
}
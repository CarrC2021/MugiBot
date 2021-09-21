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
        public static async Task CreatePlaylist(string name, string filePath)
        {
            if (File.Exists(filePath))
                return;
            File.Create(filePath);
        }

        public static async Task DeletePlaylist(string name, string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public static async Task AddToPlaylist(List<string> songkeys)
        {
            
        }
    }
}
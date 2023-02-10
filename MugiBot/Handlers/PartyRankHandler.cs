
using Discord;
using Discord.WebSocket;
using PartyBot.Database;
using PartyBot.DataStructs;
using PartyBot.Handlers;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PartyBot.Handlers
{
    public static class PartyRankHandler
    {
        public static async Task<Embed> CreateArtistCSV(string artist, ISocketMessageChannel channel, bool exact = false)
        {
            var tempChannel = (SocketTextChannel)channel;
            string path = Path.Combine(GlobalData.Config.RootFolderPath, $"{artist}.csv");
            using (var db = new AMQDBContext())
            {
            var artistSongs = await SearchHandler.SearchArtist(db, artist, exact);
            if (artistSongs.Count == 0)
                return await EmbedHandler.CreateErrorEmbed(artist, "Found no songs for that artist.");
            var sb = new StringBuilder();
            sb.Append("Song Name, Anime, Type, Link");
            foreach(SongTableObject song in artistSongs)
            {
                var newLine = $"{song.SongName}, {song.Show}, {song.Type}, {song.MP3}";
                sb.AppendLine(newLine);
            }
            await File.WriteAllTextAsync(path, sb.ToString());
            await tempChannel.SendFileAsync(path, "Kaza is this it?");
            }
            return await EmbedHandler.CreateBasicEmbed("Party Rank", $"Created a file named {artist}.csv", Color.Green);
        }
    }
}
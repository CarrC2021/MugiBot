using Discord;
using Discord.WebSocket;
using PartyBot.Database;
using PartyBot.Services;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Victoria;

namespace PartyBot.Handlers
{
    public static class CatboxHandler
    {
        internal static readonly HttpClient HttpClient = new HttpClient();
        public static async Task<string> DownloadMP3(string query, string musicPath)
        {
            Console.WriteLine("where to download");
            Console.WriteLine(Path.Combine(musicPath, "tempMusic"));

            using var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(query);
            response.EnsureSuccessStatusCode();

            string cutString = query[(query.LastIndexOf(".moe") + 5)..];

            string localpath = Path.Combine(musicPath, "tempMusic", cutString);

            using var wc = new WebClient();
            Uri tempurl = new Uri(query);
            var audio = await wc.DownloadDataTaskAsync(tempurl);
            await File.WriteAllBytesAsync(localpath, audio);
            return localpath;
        }
        public static async Task<LavaTrack> DownloadAndMakeTrack(SongTableObject song, string musicPath, LavaNode _lavaNode)
        {
            string MP3Link = await DownloadMP3(song.MP3, musicPath);
            Console.WriteLine(MP3Link);
            var search = await _lavaNode.SearchAsync(MP3Link);
            LavaTrack track = search.Tracks.FirstOrDefault();
            Console.WriteLine(track.Url);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            return OverWriteTrackData(track, song.SongName, song.Artist, song.Show, song.Type);
        }

        public static LavaTrack OverWriteTrackData(LavaTrack track, string songName, string author, string Show, string Type)
        {
            LavaTrack toReturn = new LavaTrack(track.Hash, track.Id,
                Show + " " + Type + " " + songName, author, track.Url, track.Position, track.Duration.Ticks, track.CanSeek, track.IsStream);
            return toReturn;
        }
        public static async Task<String> QueueRadioSong(SongTableObject sto, SocketGuild guild, LavaNode _lavaNode, string musicPath)
        {
            if (sto == null)
            {
                await LoggingService.LogInformationAsync("QueueRadioSong", "Somehow a null songtableobject was passed.");
                return "Please ping the creator of this app to show this error message. A null SongTableObject was passed to QueueRadioSong.";
            }
            try
            {
                var track = await DownloadAndMakeTrack(sto, musicPath, _lavaNode);
                var player = _lavaNode.GetPlayer(guild);
                player.Queue.Enqueue(track);
            }
            catch (Exception ex)
            {
                await LoggingService.LogInformationAsync("Queue Catbox Song", ex.Message);
            }
            return "success";
        }
    }
}


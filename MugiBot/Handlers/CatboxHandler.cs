using Discord;
using Discord.WebSocket;
using PartyBot.Database;
using PartyBot.Services;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Victoria;

namespace PartyBot.Handlers
{
    public static class CatboxHandler
    {
        public static async Task<String> DownloadMP3(string query, string musicPath, HttpClient httpClient)
        {
            await LoggingService.LogAsync("DownloadMP3", LogSeverity.Verbose, $"Attempting to download {query}.");
            string[] split = query.Split("/");
            string cutString = split.Last().Replace(".mp3", "");
            string localpath = Path.Combine(musicPath, "tempMusic", cutString);
            try 
            {
                var response = await httpClient.GetByteArrayAsync(query);
                await File.WriteAllBytesAsync(localpath, response);
                await LoggingService.LogAsync("DownloadMP3", LogSeverity.Verbose, $"Successfully Downloaded {query}");
                return localpath;
            }
            catch (HttpRequestException ex) 
            {
                await LoggingService.LogAsync("DownloadMP3", LogSeverity.Critical, $"Failed to download {query}.\n Error Source: {ex.Source} \n Error message: {ex.Message} \n Error Stack Trace {ex.StackTrace}");
                await LoggingService.LogAsync("DownloadMP3", LogSeverity.Critical, $"Help Link: {ex.HelpLink}\n Status Code: {ex.HResult}\n method that threw exception: {ex.TargetSite}");
                return "failed to download";
            }
        }
        public static async Task<LavaTrack> DownloadAndMakeTrack(SongTableObject song, string musicPath, LavaNode _lavaNode, HttpClient client)
        {
            var path = await DownloadMP3(song.MP3, musicPath, client);
            var search = await _lavaNode.SearchAsync(path);
            if (search.Tracks.FirstOrDefault() == null)
                await LoggingService.LogCriticalAsync("Lavanode.SearchAsync", "returned a null track");
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
        public static async Task QueueRadioSong(SongTableObject sto, SocketGuild guild, LavaNode _lavaNode, string musicPath, HttpClient client)
        {
            if (sto == null)
            {
                await LoggingService.LogCriticalAsync("QueueRadioSong", "Somehow a null songtableobject was passed.");
            }
            var track = await DownloadAndMakeTrack(sto, musicPath, _lavaNode, client);
            var player = _lavaNode.GetPlayer(guild);
            player.Queue.Enqueue(track);
        }
    }
}


using Discord;
using Discord.WebSocket;
using PartyBot.Database;
using PartyBot.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Responses;
using Victoria.Responses.Rest;

namespace PartyBot.Services
{
    public sealed class LavaLinkAudio
    {
        private readonly LavaNode _lavaNode;
        private readonly HttpClient client;
        private readonly char separator = Path.DirectorySeparatorChar;
        public string path;
        public List<Radio> radios;

        public string UserAgent { get; }

        public LavaLinkAudio(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
            client = new HttpClient()
            {
                BaseAddress = new Uri("https://files.catbox.moe"),
            };
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:10.0) Gecko/20100101 Firefox/10.0");
            radios = new List<Radio>();
        }

        public async Task<Embed> JoinAsync(IGuild guild, IVoiceState voiceState, ITextChannel textChannel)
        {
            if (_lavaNode.HasPlayer(guild))
                return await EmbedHandler.CreateErrorEmbed("Music, Join", "I'm already connected to a voice channel!");

            if (voiceState.VoiceChannel is null)
                return await EmbedHandler.CreateErrorEmbed("Music, Join", "You must be connected to a voice channel!");

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);
                await LoggingService.LogInformationAsync("Music, Join", $"Joined {voiceState.VoiceChannel.Name}.");
                return await EmbedHandler.CreateBasicEmbed("Music, Join", $"Joined {voiceState.VoiceChannel.Name}.", Color.Green);
            }
            catch (Exception ex)
            {
                await LoggingService.LogCriticalAsync("Join command", ex.Message, ex);
                return await EmbedHandler.CreateErrorEmbed("Music, Join", ex.Message);
            }
        }

        public async Task<LavaTrack> FindTrackAsync(string query)
        {
            //Find The Track the User requested.
            LavaTrack track;
            SearchResponse search;
            if (query.Contains("catbox.moe") || query.Contains("catbox.video"))
            {
                var localpath = await CatboxHandler.DownloadMP3(query, path, client);
                search = await _lavaNode.SearchAsync(localpath);
            }
            else
            {
                search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ?
                await _lavaNode.SearchAsync(query)
                : await _lavaNode.SearchYouTubeAsync(query);
            }

            //If we couldn't find anything, tell the user.
            if (search.LoadStatus == LoadStatus.NoMatches)
                return null;

            //Get the first track from the search results.
            //TODO: Add a 1-5 list for the user to pick from. (Like Fredboat)
            track = search.Tracks.FirstOrDefault();
            await LoggingService.LogInformationAsync("Music, Find", $"Found track {track.Title} by {track.Author}.");
            return track;
        }

        /*This is ran when a user uses either the command Join or Play
            I decided to put these two commands as one, will probably change it in future. 
            Task Returns an Embed which is used in the command call.. */
        public async Task<Embed> PlayAsync(SocketGuildUser user, IGuild guild, string query, SongTableObject sto = null)
        {
            //Check If User Is Connected To Voice Cahnnel.
            if (user.VoiceChannel == null)
                return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", "You Must First Join a Voice Channel.");

            //Check the guild has a player available.
            if (!_lavaNode.HasPlayer(guild))
                return await EmbedHandler.CreateErrorEmbed("Music, Play", "I'm not connected to a voice channel.");

            try
            {
                //Get the player for that guild.
                var player = _lavaNode.GetPlayer(guild);

                //Find The Track the User requested.
                LavaTrack track;
                if (sto == null)
                {
                    track = await FindTrackAsync(query);
                    if (track == null)
                        return await EmbedHandler.CreateErrorEmbed("Music", $"I wasn't able to find anything for {query}.");
                }
                else
                {
                    track = await CatboxHandler.DownloadAndMakeTrack(sto, path, _lavaNode, client);
                }
                //If the Bot is already playing music, or if it is paused but still has music in the playlist, Add the requested track to the queue.
                if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    player.Queue.Enqueue(track);
                    await LoggingService.LogInformationAsync("Music", $"{track.Title} has been added to the music queue.");
                    return await EmbedHandler.CreateBasicEmbed("Music", $"{track.Title} has been added to queue.", Color.Blue);
                }

                //Player was not playing anything, so lets play the requested track.
                await player.PlayAsync(track);
                await LoggingService.LogInformationAsync("Music", $"Bot Now Playing: {track.Title}");
                return await EmbedHandler.CreateBasicEmbed("Music", $"Now Playing: {track.Title} by {track.Author}", Color.Blue);
            }
            catch (Exception ex)
            {
                await LoggingService.LogCriticalAsync("Music, Play", $"Error Message: {ex.Message}\n Error Source: {ex.Source}");
                return await EmbedHandler.CreateErrorEmbed("Music, Play", $"{ex.Message} \n {query}");
            }

        }

        /*This is ran when a user uses the command Leave.
            Task Returns an Embed which is used in the command call. */
        public async Task<Embed> LeaveAsync(IGuild guild)
        {
            try
            {
                //Get The Player Via GuildID.
                var player = _lavaNode.GetPlayer(guild);

                //If The Player has not been created then tell the user that.
                if (player == null)
                    return await EmbedHandler.CreateBasicEmbed("Music", $"Not connected to a voice channel.", Color.Red);

                //If The Player is playing, Stop it.
                if (player.PlayerState is PlayerState.Playing)
                    await player.StopAsync();

                var radio = RadioHandler.FindRadio(radios, (SocketGuild)guild);
                radios.Remove(radio);

                //Leave the voice channel.
                await _lavaNode.LeaveAsync(player.VoiceChannel);
                await LoggingService.LogInformationAsync("Music", $"Bot has left.");
                return await EmbedHandler.CreateBasicEmbed("Music", $"I've left. Thank you for playing music.", Color.Blue);
            }
            catch (InvalidOperationException ex)
            {
                await LoggingService.LogCriticalAsync("Music, Leave", ex.Message, ex);
                return await EmbedHandler.CreateErrorEmbed("Music, Leave", ex.Message);
            }
        }

        /*This is ran when a user uses the command Queue
            Task Returns an Embed which is used in the command call. */
        public async Task<Embed> QueueAsync(SocketGuild guild, ISocketMessageChannel channel)
        {
            try
            {
                /* Create a string builder we can use to format how we want our list to be displayed. */
                var descriptionBuilder = new StringBuilder();

                var radio = RadioHandler.FindRadio(radios, guild);
                var queue = new List<SongTableObject>();
                if (radio != null)
                    queue = radio.GetQueue();

                /* Get The Player and make sure it isn't null. */
                var player = _lavaNode.GetPlayer(guild);
                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.");

                /*If the queue count is less than 1 and the current track IS NOT null then we wont have a list to reply with.
                    In this situation we simply return an embed that displays the current track instead. */
                if (player.Queue.Count < 1 && queue.Count() < 1 && player.Track != null)
                    return await EmbedHandler.CreateBasicEmbed($"Current Song: {player.Track.Title} by {player.Track.Author}", $"Nothing else queued", Color.Blue);
                /* Now we know if we have something in the queue worth replying with, so we iterate through all the Tracks in the queue.
                 *  Next Add the Track title and the url however make use of Discords Markdown feature to display everything neatly.
                    This trackNum variable is used to display the number in which the song is in place. (Start at 2 because we're including the current song.*/
                string title = "Nothing currently playing\n";
                var trackNum = 1;
                if (player.PlayerState is PlayerState.Playing)
                {
                    title = $"Now Playing: {player.Track.Title} by {player.Track.Author}\n";
                    trackNum = 2;
                }
                foreach (LavaTrack track in player.Queue)
                {
                    if (($"{title} {descriptionBuilder}\n" + $"{trackNum}: {track.Title}\n").Length >= 2048)
                        break;
                    descriptionBuilder.Append($"{trackNum}: {track.Title}\n");
                    trackNum++;
                }
                foreach (SongTableObject song in queue)
                {
                    if (($"{title} {descriptionBuilder}\n" + $"{trackNum}: {song.PrintSong()}\n").Length >= 2048)
                        break;
                    descriptionBuilder.Append($"{trackNum}: {song.PrintSong()}\n");
                    trackNum++;
                }

                return await EmbedHandler.CreateBasicEmbed("Music, List", $"{title} \n{descriptionBuilder}\n", Color.Blue);
            }
            catch (Exception ex)
            {
                await LoggingService.LogAsync(ex.Source, LogSeverity.Debug, ex.Message, ex);
                return await EmbedHandler.CreateErrorEmbed("Music, List", ex.Message);
            }

        }

        /*This is ran when a user uses the command Skip 
            Task Returns an Embed which is used in the command call. */
        public async Task<Embed> SkipTrackAsync(SocketGuildUser user, SocketGuild guild, Radio radio)
        {
            var player = _lavaNode.GetPlayer(guild);
            /* Check if the player exists */
            if (player == null)
                return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.");
            /* Check The queue, if it is less than one (meaning we only have the current song available to skip) it wont allow the user to skip.
                 User is expected to use the Stop command if they're only wanting to skip the current song. */
            try
            {
                /* Save the current song for use after we skip it. */
                var currentTrack = player.Track;
                var queue = player.Queue;
                if (player.Queue.Count == 0)
                {
                    var song = await radio.NextSong();
                    if (song != null)
                    {
                        await CheckDeleteTempMusicFile(currentTrack);
                        await PlayAsync(user, guild, song.Key, song);
                        await player.SkipAsync();
                        return await EmbedHandler.CreateBasicEmbed("Music Skip", $"I have successfully skipped {currentTrack.Title}\n Now playing {player.Track.Title} by {player.Track.Author}", Color.Blue);
                    }
                    //If the radio is not set to autoplay then just return StopAsync
                    if (!radio.Autoplay)
                        return await StopAsync(guild);

                    await StopAsync(guild);
                    return await StartRadio(radio, user);
                }
                /* Skip the current song. */
                await player.SkipAsync();

                await CheckDeleteTempMusicFile(currentTrack);

                await LoggingService.LogInformationAsync("Music", $"Bot skipped: {currentTrack.Title}");
                return await EmbedHandler.CreateBasicEmbed("Music Skip", $"I have successfully skipped {currentTrack.Title}\n Now playing {player.Track.Title} by {player.Track.Author}", Color.Blue);
            }
            catch (Exception ex)
            {
                await LoggingService.LogAsync(ex.Source, LogSeverity.Debug, ex.Message, ex);
                return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.Message);
            }

        }

        /*This is ran when a user uses the command Stop 
            Task Returns an Embed which is used in the command call. */
        public async Task<Embed> StopAsync(SocketGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);

                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.");

                /* Check if the player exists, if it does, check if it is playing.
                     If it is playing, we can stop.*/
                if (player.PlayerState is PlayerState.Playing)
                    await player.StopAsync();

                await ClearQueue(player, guild);

                await LoggingService.LogInformationAsync("Music", $"Bot has stopped playback.");
                return await EmbedHandler.CreateBasicEmbed("Music Stop", "I Have stopped playback & the playlist has been cleared.", Color.Blue);
            }
            catch (Exception ex)
            {
                await LoggingService.LogAsync(ex.Source, LogSeverity.Debug, ex.Message, ex);
                return await EmbedHandler.CreateErrorEmbed("Music, Stop", ex.Message);
            }
        }

        // Clears the entire queue for the LavaPlayer and the server
        public async Task ClearQueue(LavaPlayer player, SocketGuild guild)
        {
            foreach (LavaTrack track in player.Queue)
            {
                await CheckDeleteTempMusicFile(track);
                player.Queue.Remove(track);
            }
            await Task.Run(() => RadioHandler.FindRadio(radios, guild).Queue.Clear());
        }

        /*This is ran when a user uses the command Volume 
            Task Returns a String which is used in the command call. */
        public async Task<string> SetVolumeAsync(IGuild guild, int volume)
        {
            if (volume > 150 || volume <= 0)
                return $"Volume must be between 1 and 150.";

            try
            {
                var player = _lavaNode.GetPlayer(guild);
                await player.UpdateVolumeAsync((ushort)volume);
                await LoggingService.LogInformationAsync("Music", $"Bot Volume set to: {volume}");
                return $"Volume has been set to {volume}.";
            }
            catch (InvalidOperationException ex)
            {
                await LoggingService.LogAsync(ex.Source, LogSeverity.Debug, ex.Message, ex);
                return ex.Message;
            }
        }

        public async Task<string> PauseAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (!(player.PlayerState is PlayerState.Playing))
                {
                    await player.PauseAsync();
                    return $"There is nothing to pause.";
                }

                await player.PauseAsync();
                return $"**Paused:** {player.Track.Title}, what a bamboozle.";
            }
            catch (InvalidOperationException ex)
            {
                await LoggingService.LogAsync(ex.Source, LogSeverity.Debug, ex.Message, ex);
                return ex.Message;
            }
        }

        public async Task<string> ResumeAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);

                if (player.PlayerState is PlayerState.Paused)
                    await player.ResumeAsync();

                return $"**Resumed:** {player.Track.Title}";
            }
            catch (InvalidOperationException ex)
            {
                await LoggingService.LogAsync(ex.Source, LogSeverity.Debug, ex.Message, ex);
                return ex.Message;
            }
        }

        public async Task OnTrackStuck(TrackStuckEventArgs args)
        {
            await LoggingService.LogInformationAsync("TrackStuck", $"{args.Track.Title} by {args.Track.Author} stuck");
        }

        public async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            await LoggingService.LogInformationAsync("TrackEnded", $"{args.Track.Title} by {args.Track.Author} ended for reason: {args.Reason}.");
            if (!args.Reason.ShouldPlayNext())
                return;

            var player = args.Player;
            Radio guildRadio = RadioHandler.FindRadio(
                    radios,
                    player.TextChannel.Guild as SocketGuild
                );

            await CheckDeleteTempMusicFile(args.Track, guildRadio);
            await LoggingService.LogInformationAsync("TrackEnded", $"Removed the file for {args.Track}");
            
            string toPrint = "Now Playing:";
            if (guildRadio != null && player.Queue.Count < 2)
            {
                await RadioQueue(guildRadio);
                if (!guildRadio.CurrPlayers.Equals("any"))
                    toPrint = $"You are Listening to {guildRadio.GetPlayers()} Radio. Now Playing:";
            }

            if (!player.Queue.TryDequeue(out var queueable))
            {
                await player.TextChannel.SendMessageAsync(
                embed: await EmbedHandler.CreateBasicEmbed("Queue Empty", "Playback has finished", Color.Blue));
                return;
            }

            if (queueable is not LavaTrack track)
            {
                await player.TextChannel.SendMessageAsync(
                    embed: await EmbedHandler.CreateErrorEmbed("Lavalink Error", "Next item in queue is not a track"));
                return;
            }
            await player.PlayAsync(track);
            await player.TextChannel.SendMessageAsync(
                embed: await EmbedHandler.CreateBasicEmbed(toPrint, $"{track.Title} by {track.Author}", Color.Blue));
        }

        public async Task<Embed> QueueCatboxFromDB(string key, SocketGuildUser user, IGuild guild)
        {
            var song = await DBSearchService.UseSongKey(key);
            try
            {
                var embed = await PlayAsync(user, guild, song.MP3, song);
                return embed;
            }
            catch (Exception ex)
            {
                await LoggingService.LogCriticalAsync("QueueCatboxFromDB", ex.Message, ex);
                return await EmbedHandler.CreateErrorEmbed("Catbox Song", "Something went terribly wrong");
            }
        }

        public async Task<Embed> StartRadio(Radio radio, SocketGuildUser user)
        {
            try
            {
                if (!radio.IsQueueEmpty())
                {
                    var nextSong = await radio.NextSong();
                    if (Uri.IsWellFormedUriString(nextSong.MP3, UriKind.Absolute))
                        return await PlayAsync(user, radio.Guild, nextSong.MP3, nextSong);
                }
                radio.Autoplay = true;
                var song = radio.GetRandomSong();
                if (Uri.IsWellFormedUriString(song.MP3, UriKind.Absolute))
                    return await PlayAsync(user, radio.Guild, song.MP3, song);
            }
            catch (Exception ex)
            {
                await LoggingService.LogAsync(ex.Source, LogSeverity.Debug, ex.Message, ex);
            }
            return await EmbedHandler.CreateErrorEmbed("Radio", "Seems like I could not find any songs that meet the search criteria.");
        }

        public async Task RadioQueue(Radio radio)
        {
            var nextSong = new SongTableObject();
            try
            {
                // First try to see if the queue is not empty.
                if (!radio.IsQueueEmpty())
                {
                    nextSong = await radio.NextSong();
                    await CatboxHandler.QueueRadioSong(nextSong, radio.Guild, _lavaNode, path, client);
                    return;
                }
                // If there is nothing in the queue and autoplay is on then queue a random song from the song selection.
                if (radio.Autoplay)
                {
                    nextSong = radio.GetRandomSong();
                    await CatboxHandler.QueueRadioSong(nextSong, radio.Guild, _lavaNode, path, client);
                }

            }
            catch (Exception ex)
            {
                await LoggingService.LogAsync(ex.Source, LogSeverity.Debug, ex.Message, ex);
                await radio.Channel.SendMessageAsync(embed: await
                    EmbedHandler.CreateErrorEmbed("Radio", $"Something went wrong trying to queue a song from the radio. The song that failed was "
                    +$"{nextSong.PrintSong()}.\n It's url is {nextSong.MP3}"));
            }
        }

        public async Task CheckDeleteTempMusicFile(LavaTrack track, Radio radio = null)
        {
            if (track.Url.Contains($"{separator}tempMusic"))
            {
                try
                {
                    FileSystemInfo fileInfo = new FileInfo(track.Url);
                    await Task.Run(() => fileInfo.Delete());
                }
                catch (Exception ex)
                {
                    await LoggingService.LogAsync(ex.Source, LogSeverity.Debug, ex.Message, ex);
                }
            }
        }

        public async Task<Embed> LoadPlaylist(Radio radio, SocketGuildUser user, ISocketMessageChannel channel, string name, string type = "default")
        {
            // Check If User Is Connected To Voice Cahnnel.
            if (user.VoiceChannel == null)
                return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", "You Must First Join a Voice Channel.");

            // Check the guild has a player available.
            if (!_lavaNode.HasPlayer(radio.Guild))
                return await EmbedHandler.CreateErrorEmbed("Music, Play", "I'm not connected to a voice channel.");

            try
            {
                var fileName = Path.Combine(path, "playlists", name);
                var playlistPath = "";
                if (type.Equals("default"))
                    playlistPath = Path.Combine(path, "playlists", name);
                else
                    playlistPath = Path.Combine(path, "playlists", type, name);
                if (!File.Exists(playlistPath))
                    return await EmbedHandler.CreateErrorEmbed("Playlists", $"Could not find playlist with the name {name}");

                List<String> playlist = await PlaylistHandler.LoadPlaylist(playlistPath);

                Random rnd = new Random();
                for (int i = 0; i < playlist.Count; i++)
                {
                    int k = rnd.Next(0, i);
                    var key = playlist[k];
                    playlist[k] = playlist[i];
                    playlist[i] = key;
                }

                // Queue the first song immediately
                var song = new SongTableObject();
                for (int i = 0; i < 3; i++)
                {
                    if (playlist.Count == 0)
                        return await EmbedHandler.CreateBasicEmbed("Playlists", $"Loaded {name}", Color.Blue);
                    song = await DBSearchService.UseSongKey(playlist.FirstOrDefault());
                    await radio.Channel.SendMessageAsync(embed: await PlayAsync(user, radio.Guild, song.Key, song));
                    playlist.Remove(song.Key);
                }

                // Now we will place the rest of the songs in a queue
                var songs = new List<SongTableObject>();
                foreach (string key in playlist)
                    songs.Add(await DBSearchService.UseSongKey(key));
                await radio.PopulateQueue(songs);
                return await EmbedHandler.CreateBasicEmbed("Playlists", $"Loaded {name}", Color.Blue);
            }
            catch (Exception ex)
            {
                await LoggingService.LogAsync(ex.Source, LogSeverity.Debug, ex.Message, ex);
                return await EmbedHandler.CreateErrorEmbed("Playlists", ex.Message);
            }
        }
        public async Task<Embed> Loop(Radio radio, SocketGuildUser user, ISocketMessageChannel channel, int numTimes)
        {
            numTimes = Math.Abs(numTimes);
            var embed = await CheckVoice(radio, user, channel);
            if (embed != null)
                return embed;

            var currRadioQueue = Clone<List<SongTableObject>>(radio.GetQueue());
            for (int i = 0; i < numTimes; i++)
            {
                foreach (SongTableObject song in currRadioQueue)
                {
                    radio.Queue.Append(song);
                }
            }

            return await EmbedHandler.CreateBasicEmbed("Loop", $"The current queue will be looped through {numTimes} times.", Color.Blue);
        }

        public static T Clone<T>(T source)
        {

            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, source);
                ms.Seek(0, SeekOrigin.Begin);
                return (T)serializer.ReadObject(ms);
            }
        }

        public async Task<Embed> CheckVoice(Radio radio, SocketGuildUser user, ISocketMessageChannel channel)
        {
            // Check If User Is Connected To Voice Cahnnel.
            if (user.VoiceChannel == null)
                return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", "You Must First Join a Voice Channel.");

            // Check the guild has a player available.
            if (!_lavaNode.HasPlayer(radio.Guild))
                return await EmbedHandler.CreateErrorEmbed("Music, Play", "I'm not connected to a voice channel.");

            return null;
        }
    }
}

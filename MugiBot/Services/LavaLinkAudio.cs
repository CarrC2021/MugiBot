using Discord;
using Discord.WebSocket;
using PartyBot.Database;
using PartyBot.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Responses.Rest;

namespace PartyBot.Services
{
    public sealed class LavaLinkAudio
    {
        private readonly LavaNode _lavaNode;
        private readonly char separator = Path.DirectorySeparatorChar;
        private readonly string path;
        private readonly DBManager _db;
        public List<Radio> radios;

        public LavaLinkAudio(LavaNode lavaNode, DBManager _dbmanager)
        {
            _lavaNode = lavaNode;
            path = Path.GetDirectoryName(System.Reflection.
            Assembly.GetExecutingAssembly().GetName().CodeBase).Replace($"{separator}bin{separator}Debug{separator}netcoreapp3.1", "").Replace($"file:{separator}", "");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                path = separator + path;
            }
            _db = _dbmanager;
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
                return await EmbedHandler.CreateBasicEmbed("Music, Join", $"Joined {voiceState.VoiceChannel.Name}.", Color.Green);
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Join", ex.Message);
            }
        }

        public async Task<LavaTrack> FindTrackAsync(string query)
        {
            //Find The Track the User requested.
            LavaTrack track;
            SearchResponse search;
            if (query.Contains("catbox.moe"))
            {
                query = await CatboxHandler.DownloadMP3(query, path);
                search = await _lavaNode.SearchAsync(query);
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
                    track = await CatboxHandler.DownloadAndMakeTrack(sto, path, _lavaNode);
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
            //If after all the checks we did, something still goes wrong. Tell the user about it so they can report it back to us.
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Play", ex.Message);
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

                //if The Player is playing, Stop it.
                if (player.PlayerState is PlayerState.Playing)
                    await player.StopAsync();

                //Leave the voice channel.
                await _lavaNode.LeaveAsync(player.VoiceChannel);
                await LoggingService.LogInformationAsync("Music", $"Bot has left.");
                return await EmbedHandler.CreateBasicEmbed("Music", $"I've left. Thank you for playing music.", Color.Blue);
            }
            //Tell the user about the error so they can report it back to us.
            catch (InvalidOperationException ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Leave", ex.Message);
            }
        }

        /*This is ran when a user uses the command List 
            Task Returns an Embed which is used in the command call. */
        public async Task<Embed> ListAsync(SocketGuild guild, ISocketMessageChannel channel)
        {
            try
            {
                /* Create a string builder we can use to format how we want our list to be displayed. */
                var descriptionBuilder = new StringBuilder();

                var radio = RadioHandler.FindRadio(radios, guild);
                var radioQueue = new List<string>();
                if (radio != null)
                    radioQueue = await radio.PrintQueue();

                /* Get The Player and make sure it isn't null. */
                var player = _lavaNode.GetPlayer(guild);
                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.");

                if (!(player.PlayerState is PlayerState.Playing))
                    return await EmbedHandler.CreateErrorEmbed("Music, List", "Player doesn't seem to be playing anything right now.");
                /*If the queue count is less than 1 and the current track IS NOT null then we wont have a list to reply with.
                    In this situation we simply return an embed that displays the current track instead. */
                if (player.Queue.Count < 1 && player.Track != null)
                    return await EmbedHandler.CreateBasicEmbed($"Now Playing: {player.Track.Title}", $"Not sure what to put here", Color.Blue);
                /* Now we know if we have something in the queue worth replying with, so we iterate through all the Tracks in the queue.
                 *  Next Add the Track title and the url however make use of Discords Markdown feature to display everything neatly.
                    This trackNum variable is used to display the number in which the song is in place. (Start at 2 because we're including the current song.*/
                var trackNum = 2;
                foreach (LavaTrack track in player.Queue)
                {
                    descriptionBuilder.Append($"{trackNum}: {track.Title}\n");
                    trackNum++;
                }
                foreach (string song in radioQueue)
                {
                    descriptionBuilder.Append($"{trackNum}: {radioQueue}\n");
                    trackNum++;
                }

                return await EmbedHandler.CreateBasicEmbed("Music Playlist", $"Now Playing: {player.Track.Title} \n{descriptionBuilder}\n", Color.Blue);
            }
            catch (Exception ex)
            {
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
                        await CheckDeleteTempMusicFile(currentTrack.Url);
                        await PlayAsync(user, guild, song.Key, song);
                        await player.SkipAsync();
                        return await EmbedHandler.CreateBasicEmbed("Music Skip", $"I have successfully skipped {currentTrack.Title}\n Now playing {player.Track.Title} by {player.Track.Author}", Color.Blue);
                    }
                    //If the radio is off then just return StopAsync
                    if (!radio.RadioMode)
                        return await StopAsync(guild);

                    await StopAsync(guild);
                    return await StartRadio(radio, user);
                }
                /* Skip the current song. */
                await player.SkipAsync();

                await CheckDeleteTempMusicFile(currentTrack.Url);

                await LoggingService.LogInformationAsync("Music", $"Bot skipped: {currentTrack.Title}");
                return await EmbedHandler.CreateBasicEmbed("Music Skip", $"I have successfully skipped {currentTrack.Title}\n Now playing {player.Track.Title} by {player.Track.Author}", Color.Blue);
            }
            catch (Exception ex)
            {
                await LoggingService.LogInformationAsync(ex.Source, ex.Message + "\n" + ex.StackTrace);
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

                foreach (LavaTrack track in player.Queue)
                {
                    await CheckDeleteTempMusicFile(track.Url);
                    player.Queue.Remove(track);
                    await RadioHandler.FindRadio(radios, guild).DeQueueAll();
                }

                await LoggingService.LogInformationAsync("Music", $"Bot has stopped playback.");
                return await EmbedHandler.CreateBasicEmbed("Music Stop", "I Have stopped playback & the playlist has been cleared.", Color.Blue);
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Stop", ex.Message);
            }
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
                return ex.Message;
            }
        }

        public async Task TrackEnded(TrackEndedEventArgs args)
        {
            if (!args.Reason.ShouldPlayNext())
                return;

            await CheckDeleteTempMusicFile(args.Track.Url);

            Radio guildRadio = RadioHandler.FindRadio(
                    radios,
                    args.Player.TextChannel.Guild as SocketGuild
                );
            string toPrint = "Now Playing:";
            if (guildRadio != null && args.Player.Queue.Count < 3)
            {                    
                await RadioQueue(guildRadio);
                if (!guildRadio.CurrPlayers.Equals("any"))
                    toPrint = $"You are Listening to {guildRadio.CurrPlayers} Radio. Now Playing:";
            }

            if (!args.Player.Queue.TryDequeue(out var queueable))
            {
                await args.Player.TextChannel.SendMessageAsync("Playback Finished.");
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                await args.Player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
                return;
            }
            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync(
                embed: await EmbedHandler.CreateBasicEmbed(toPrint, $"{track.Title} by {track.Author}", Color.Blue));
        }

        public async Task<Embed> QueueCatboxFromDB(string key, SocketGuildUser user, IGuild guild)
        {
            SongTableObject song = await DBSearchService.UseSongKey(key);
            return await PlayAsync(user, guild, song.MP3, song);
        }

        public async Task<Embed> StartRadio(Radio radio, SocketGuildUser user)
        {
            try
            {
                radio.RadioMode = true;
                var song = radio.GetRandomSong();
                if (Uri.IsWellFormedUriString(song.MP3, UriKind.Absolute))
                {
                    return await PlayAsync(user, radio.Guild, song.MP3, song);
                }
            }
            catch (Exception ex)
            {
                await LoggingService.LogInformationAsync(ex.Message, ex.StackTrace);
            }
            return await EmbedHandler.CreateErrorEmbed("Radio", "Seems like I could not find any songs that meet the search criteria.");
        }

        public async Task RadioQueue(Radio radio)
        {
            try
            {
                // First try to see if there is anything queued up.
                var outVal = await radio.NextSong();
                if (outVal != null)
                    await CatboxHandler.QueueRadioSong(outVal, radio.Guild, _lavaNode, path);
                // If there is nothing then try to queue from the random radio selection.
                outVal = radio.GetRandomSong();
                if (outVal != null)
                    await CatboxHandler.QueueRadioSong(radio.GetRandomSong(), radio.Guild, _lavaNode, path);
            }
            catch (Exception ex)
            {
                await radio.Channel.SendMessageAsync(embed: await 
                    EmbedHandler.CreateErrorEmbed("Radio", "Something went wrong trying to queue a song from the radio."));
                await LoggingService.LogInformationAsync(ex.Source, ex.Message + "\n" + ex.StackTrace);
            }
        }

        public async Task CheckDeleteTempMusicFile(string filePath)
        {
            if (filePath.Contains($"{separator}tempMusic"))
            {
                try
                {
                    FileSystemInfo fileInfo = new FileInfo(filePath);
                    fileInfo.Delete();
                }
                catch (Exception ex)
                {
                    await LoggingService.LogInformationAsync(ex.Source, ex.Message);
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
                var fileName = PlaylistHandler.SearchPlaylistDirectories(Path.Combine(path, "playlists"), name);
                var playlistPath = "";
                if (type.Equals("default"))
                    playlistPath = Path.Combine(path, "playlists", name);
                else
                    playlistPath = Path.Combine(path, "playlists", type, name);
                if (!File.Exists(playlistPath))
                    return await EmbedHandler.CreateErrorEmbed("Playlists", $"Could not find playlist with the name {name}");
                
                var playlist = await PlaylistHandler.LoadPlaylist(playlistPath);

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
                    await PlayAsync(user, radio.Guild, song.Key, song);
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
                await LoggingService.LogInformationAsync("Playlists", ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace);
                return await EmbedHandler.CreateErrorEmbed("Playlists", ex.Message);
            }
        }
    }
}

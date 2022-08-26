using System.Net.Mime;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PartyBot.Database;
using PartyBot.Handlers;
using PartyBot.Services;
using System.Threading.Tasks;

namespace PartyBot.Modules
{
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        /* Get our AudioService from DI */
        public LavaLinkAudio AudioService { get; set; }
        public DBManager DBManager { get; set; }
        public AnilistService AnilistService { get; set; }

        /* All the below commands are ran via Lambda Expressions to keep this file as neat and closed off as possible. 
              We pass the AudioService Task into the section that would normally require an Embed as that's what all the
              AudioService Tasks are returning. */

        [Command("Join")]
        [Summary("Joins the user's current channel.")]
        public async Task JoinAndPlay()
            => await ReplyAsync(embed: await AudioService.JoinAsync(Context.Guild, Context.User as IVoiceState, Context.Channel as ITextChannel));

        [Command("Leave")]
        [Summary("Disconnects from the current channel.")]
        public async Task Leave()
            => await ReplyAsync(embed: await AudioService.LeaveAsync(Context.Guild));

        [Command("Play")]
        [Summary("Takes a search term and plays the first result on youtube if the bot is connected to a channel. Alternatively," +
            "if a mp3 catbox link is provided the bot will download and play that mp3.")]
        public async Task Play([Remainder] string search)
            => await ReplyAsync(embed: await AudioService.PlayAsync(Context.User as SocketGuildUser, Context.Guild, search));

        [Command("Stop")]
        [Summary("Stops current playback and clears queue.")]
        public async Task Stop()
            => await ReplyAsync(embed: await AudioService.StopAsync(Context.Guild));

        [Command("List")]
        [Summary("Placeholder for now")]
        public async Task List()
            => await ReplyAsync(embed: await EmbedHandler.CreateBasicEmbed("List", "!list was changed to !queue, I might use !list for something else in the future", Color.Blue));

        [Command("Queue")]
        [Summary("Prints the queue.")]
        public async Task Queue()
            => await ReplyAsync(embed: await AudioService.QueueAsync(Context.Guild, Context.Channel));

        [Command("Skip")]
        [Summary("Skips the current song if there is another song in the queue.")]
        public async Task Skip()
            => await ReplyAsync(embed: await AudioService.SkipTrackAsync(Context.User as SocketGuildUser, Context.Guild, RadioHandler.FindRadio(AudioService.radios, Context.Guild)));

        [Command("Volume")]
        [Summary("Takes an integer and adjusts volume from 0 to 150.")]
        public async Task Volume(int volume)
            => await ReplyAsync(await AudioService.SetVolumeAsync(Context.Guild, volume));

        [Command("Pause")]
        [Summary("Pauses current playback.")]
        public async Task Pause()
            => await ReplyAsync(await AudioService.PauseAsync(Context.Guild));

        [Command("Resume")]
        [Summary("Resumes current playback.")]
        public async Task Resume()
            => await ReplyAsync(await AudioService.ResumeAsync(Context.Guild));

        [Command("PlayKey")]
        [Summary("Takes a key and searches the database for the song."
            + " If the key is correct, then it will start playing. "
            + "If you don't know what the format for a key looks like use the searchdb command.")]
        public async Task QueueCatboxFromDB([Remainder] string key)
            => await ReplyAsync(embed: await AudioService.QueueCatboxFromDB(key, Context.User as SocketGuildUser, Context.Guild));
 
        [Command("LoadPlaylist")]
        [Summary("Starts the radio and will keep playing songs until you turn it off.")]
        public async Task LoadPlaylist([Remainder] string name)
        => await ReplyAsync(embed: await AudioService.LoadPlaylist(RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild), Context.User as SocketGuildUser, Context.Channel, name.ToLower()));

        [Command("Loop")]
        [Summary("Will loop what is currently in the queue the specified number of times. Note if you use !play or !playkey this command will fail to loop those." +
        " Realistically speaking I will probably never fix that, so it only works for things queued with the !load commands.")]
        public async Task Loop([Remainder] int numTimes)
        => await ReplyAsync(embed: await AudioService.Loop(RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild), Context.User as SocketGuildUser, Context.Channel, numTimes));
    }
}

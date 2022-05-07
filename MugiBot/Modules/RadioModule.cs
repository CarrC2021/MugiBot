using System.Net.Mime;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PartyBot.Database;
using PartyBot.Handlers;
using PartyBot.Services;
using System.IO;
using System.Threading.Tasks;

namespace PartyBot.Modules
{
    public class RadioModule : ModuleBase<SocketCommandContext>
    {
        /* Get our AudioService from DI */
        public LavaLinkAudio AudioService { get; set; }
        public DBManager DBManager { get; set; }
        public AnilistService AnilistService { get; set; }
        public DataService DataService { get; set; }

        [Command("RadioOff")]
        [Summary("Turns the Radio off.")]
        public async Task TurnRadioOff()
        => await ReplyAsync(embed: await RadioHandler.TurnOff(RadioHandler.FindRadio(AudioService.radios, Context.Guild)));
        [Command("RCP")]
        [Summary("Changes the player name in the Radio to the given argument.")]
        public async Task ChangePlayerName([Remainder] string playerName = "any")
        => await ReplyAsync(embed: await RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild).ChangePlayers(playerName, DBManager, AnilistService));
        [Command("RCT")]
        [Summary("Changes the type of song played by the Radio to the given argument.")]
        public async Task ChangePlayerName(int type)
        => await ReplyAsync(embed: await RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild).SetType(type, DBManager, AnilistService));
        [Command("RCT")]
        [Summary("Changes the type of song played by the Radio to the given argument.")]
        public async Task ChangePlayerNameString([Remainder] string type)
        => await ReplyAsync(embed: await RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild).SetType(type, DBManager, AnilistService));
        [Command("RLT")]
        [Summary("Lists out the types the Radio can use.")]
        public async Task ListTypes()
        => await ReplyAsync(embed: await RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild).ListTypes());
        [Command("RInfo")]
        [Summary("Print out information about the radio in this server.")]
        public async Task PrintRadioInfo()
        => await ReplyAsync(embed: await RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild).PrintRadio());
        [Command("RAL")]
        [Summary("This will add to the radio a condition to play the songs from the specified list status. For example, !ral Watching Completed Dropped Paused " +
            "will play songs that meet those conditions in the database. By default this is set Watching or completed")]
        public async Task AddRadioListStatus([Remainder] string input)
        => await ReplyAsync(embed: await RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild).AddListStatus(input.Split(), DBManager, AnilistService));
        [Command("RDL")]
        [Summary("This will remove from the radio a condition to play the songs from the specified list status. For example, !rdl Watching Completed Dropped Paused " +
            "will remove songs that meet those conditions in the database. By default this is set to Watching or completed")]
        public async Task RemoveRadioListStatus([Remainder] string input)
        => await ReplyAsync(embed: await RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild).RemoveListStatus(input.Split(), DBManager, AnilistService));
        [Command("StartRadio")]
        [Summary("Starts the radio and will keep playing songs until you turn it off.")]
        public async Task StartRadio()
        => await ReplyAsync(embed: await AudioService.StartRadio(RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild), Context.User as SocketGuildUser));

        [Command("LoadPlaylist")]
        [Summary("Starts the radio and will keep playing songs until you turn it off.")]
        public async Task LoadPlaylist([Remainder] string name)
        => await ReplyAsync(embed: await AudioService.LoadPlaylist(RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild), Context.User as SocketGuildUser, Context.Channel, name.ToLower()));

        [Command("LoadArtist")]
        [Summary("Loads all songs where the specified artist is credited.")]
        public async Task LoadArtist([Remainder] string artist)
            => await ReplyAsync(embed: await RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild).PopulateQueue(
                await PlaylistHandler.LoadSongsForQuery(artist, "artist", "any")));

        [Command("LoadArtistExact")]
        [Summary("Loads all songs where the specified artist is the only one credited.")]
        public async Task LoadArtistExact([Remainder] string artist)
            => await ReplyAsync(embed: await RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild).PopulateQueue(
                await PlaylistHandler.LoadSongsForQuery(artist, "artist", "any", true)));

        [Command("LoadShow")]
        [Summary("Loads all songs where the show contains the query specified.")]
        public async Task LoadShow([Remainder] string show)
            => await ReplyAsync(embed: await RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild).PopulateQueue(
                await PlaylistHandler.LoadSongsForQuery(show, "show", "any")));

        [Command("LoadShowEds")]
        [Summary("Loads all endings where the show contains the query specified.")]
        public async Task LoadShowED([Remainder] string show)
            => await ReplyAsync(embed: await RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild).PopulateQueue(
                await PlaylistHandler.LoadSongsForQuery(show, "show", "Ending")));

        [Command("LoadShowINS")]
        [Summary("Loads all inserts where the show contains the query specified.")]
        public async Task LoadShowINS([Remainder] string show)
            => await ReplyAsync(embed: await RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild).PopulateQueue(
                await PlaylistHandler.LoadSongsForQuery(show, "show", "Insert")));

        [Command("LoadShowOPs")]
        [Summary("Loads all openings from shows that contain the query specified.")]
        public async Task LoadShowOP([Remainder] string show)
            => await ReplyAsync(embed: await RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild).PopulateQueue(
                await PlaylistHandler.LoadSongsForQuery(show, "show", "Opening")));

        [Command("LoadShowExact")]
        [Summary("Loads all songs from the show that matches the query exactly.")]
        public async Task LoadShowExact([Remainder] string show)
            => await ReplyAsync(embed: await RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild).PopulateQueue(
                await PlaylistHandler.LoadSongsForQuery(show, "show", "any", true)));

        [Command("LoadShowEDsExact")]
        [Summary("Adds a playlist of the given name automatically populated with songs from that show.")]
        public async Task LoadShowEDExact([Remainder] string show)
            => await ReplyAsync(embed: await RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild).PopulateQueue(
                await PlaylistHandler.LoadSongsForQuery(show, "show", "Ending", true)));

        [Command("LoadShowINSExact")]
        [Summary("Loads all inserts from the show that matches the query specifically.")]
        public async Task LoadShowINSExact([Remainder] string show)
            => await ReplyAsync(embed: await RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild).PopulateQueue(
                await PlaylistHandler.LoadSongsForQuery(show, "show", "Insert", true)));

        [Command("LoadShowOPsExact")]
        [Summary("Loads all openings from the show that matches the query specifically.")]
        public async Task LoadShowOPExact([Remainder] string show)
            => await ReplyAsync(embed: await RadioHandler.FindOrCreateRadio(
                AudioService.radios, Context.Channel, Context.Guild).PopulateQueue(
                await PlaylistHandler.LoadSongsForQuery(show, "show", "Opening", true)));
    
    }
}

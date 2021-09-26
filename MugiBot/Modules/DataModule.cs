using Discord.Commands;
using PartyBot.Handlers;
using PartyBot.Services;
using System.Threading.Tasks;
using System.IO;

namespace PartyBot.Modules
{
    public class DataModule : ModuleBase<SocketCommandContext>
    {
        public DataService DataService { get; set; }
        public PlayersRulesService _playersRulesService { get; set; }

        [Command("MergePlayers")]
        [Summary("Will merge one player's stats into another players.")]
        public async Task MergeTest(string mergeFrom, string mergeInto)
            => await ReplyAsync(embed: await DataService.MergeTest(mergeFrom, mergeInto));

        [Command("ListJsons")]
        [Summary("This will list all of the Jsons in the Json folder.")]
        public async Task ListJsons()
            => await ReplyAsync(embed: await DataService.ListJsons());

        [Command("UpdateDB")]
        [Summary("This command will update the player stats database please do not uplad teams matches.")]
        public async Task UpdateDatabase()
            => await ReplyAsync(embed: await DataService.DBManager.AddAllToDatabase());

        [Command("UpdateSongDB")]
        [Summary("This command will update the song database using all files the bot has downloaded and player stats too if the file name does not include co-op, coop, or teams.")]
        public async Task UpdateSongDatabase(string expandLibraryFile)
            => await ReplyAsync(embed: await DataService.DBManager.UpdateSongDatabase(expandLibraryFile));

        [Command("CalcTotal")]
        [Summary("This will calculate and list all players' total success rate. There is an optional argument to calculate successrate by rule. Use !listrules to see what they are.")]
        public async Task CalcTotalCorrect([Remainder] string rule = "")
            => await ReplyAsync(embed: await DataService.CalcTotalCorrect(_playersRulesService, rule));

        [Command("RecommendSongs")]
        [Summary("This will recommend however many songs to the player specified that the user chooses. Example, !recommendsongs bm98 10. That will recommend 10 songs to bm98")]
        public async Task RecommendPracticeSongs(string playerName, int num = 5)
            => await ReplyAsync(embed: await DataService.RecommendPracticeSongs(Context.Channel, playerName, num, false));

        [Command("PracticeMyList")]
        [Summary("Input a player name and a number of songs and this will give you songs from your list to practice.")]
        public async Task PracticeMyList(string playerName, int num = 5)
            => await ReplyAsync(embed: await DataService.RecommendPracticeSongs(Context.Channel, playerName, num, true));

        [Command("GithubTest")]
        public async Task GithubTest(string repo, int page, int perPage)
            => await ReplyAsync(embed: await DataService.DBManager.AddSongListFilesToDataBase(await GithubHandler.ReturnJsonGists(repo, page, perPage)));


        [Command("RemoveDeadSongs")]
        public async Task RemoveDeadSongs()
            => await ReplyAsync(embed: await DataService.DBManager.RemoveDeadSongs());

        [Command("TestEmbed")]
        public async Task TestEmbed()
            => await ReplyAsync(embed: await EmbedHandler.TestingEmbedStuff());

        [Command("SearchAnilist")]
        public async Task TestAnilist()
            => await DataService.anilistService.GetCoverArtAsync("Show By Rock!!", 16311);

        [Command("CreatePlaylist")]
        [Summary("Adds a playlist of the given name.")]
        public async Task CreatePlaylist(string name)
            => await ReplyAsync(embed: await DataService.CreatePlaylist(name));

        [Command("AddToPlaylist")]
        [Summary("Enter a playlist and song key to add a song to a playlist.")]
        public async Task AddToPlaylist(string playlistName, [Remainder] string key)
            => await ReplyAsync(embed: await DataService.AddToPlaylist(playlistName.ToLower(), key));
        [Command("RemoveFromPlaylist")]
        [Summary("Removes from a playlist the song you enter.")]
        public async Task RemoveFromPlaylist(string playlistName, [Remainder] string key)
            => await ReplyAsync(embed: await DataService.RemoveFromPlaylist(playlistName.ToLower(), key));
        [Command("ShufflePlaylist")]
        [Summary("Shuffles the specified playlist.")]
        public async Task ShufflePlaylist(string playlistName)
            => await ReplyAsync(embed: await DataService.ShufflePlaylist(playlistName.ToLower()));

        [Command("PrintPlaylist")]
        [Summary("Prints the content of the specified playlist.")]
        public async Task PrintPlaylist(string playlistName)
            => await ReplyAsync(embed: await DataService.PrintPlaylist(playlistName.ToLower(), Context.Channel));
        
        [Command("CreateArtistPlaylist")]
        [Summary("Adds a playlist of the given name.")]
        public async Task CreateArtistPlaylist([Remainder] string author)
            => await ReplyAsync(embed: await PlaylistHandler.CreateArtistPlaylist(author, Path.Combine(DataService.path, "playlists", "artists", author)));
    }
}

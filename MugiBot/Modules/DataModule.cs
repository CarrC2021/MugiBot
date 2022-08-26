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

        [Command("DeleteJson")]
        [Summary("This will delete the specified json in the Json folder.")]
        public async Task DeleteJson([Remainder] string fileName)
            => await ReplyAsync(embed: await DataService.DeleteJson(fileName));

        [Command("UpdateDB")]
        [Summary("This command will update the player stats database please do not uplad teams matches.")]
        public async Task UpdateDatabase()
            => await ReplyAsync(embed: await DataService.DBManager.AddAllToDatabase());

        [Command("UpdateSongDB")]
        [Summary("This command will update the song database using all files the bot has downloaded and player stats too if the file name does not include co-op, coop, or teams.")]
        public async Task UpdateSongDatabase([Remainder] string expandLibraryFile)
            => await ReplyAsync(embed: await DataService.DBManager.UpdateSongDatabase(Context.User, expandLibraryFile));

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

        [Command("UpdateSongLink")]
        [Summary("To use this command first provide a link, then paste the songkey after to identify which song to update." +
        " In order to use this command you need Database admin priviliges.")]
        public async Task UpdateSongLink(string newLink, [Remainder] string songkey)
            => await ReplyAsync(embed: await DataService.DBManager.UpdateSongLink(songkey, newLink, Context.User.Id));

        [Command("TestEmbed")]
        public async Task TestEmbed()
            => await ReplyAsync(embed: await EmbedHandler.TestingEmbedStuff());

        [Command("SearchAnilist")]
        public async Task TestAnilist()
            => await DataService.anilistService.GetCoverArtAsync("Show By Rock!!", 16311);

        [Command("CreatePlaylist")]
        [Summary("Adds a playlist of the given name.")]
        public async Task CreatePlaylist([Remainder] string name)
            => await ReplyAsync(embed: await DataService.CreatePlaylist(name));
        
        [Command("CreatePlaylistFromGame")]
        [Summary("Creates a playlist that contains all of the songs you missed in the attached file. This method requires a file attachment to the message.")]
        public async Task CreatePlaylistFromGame([Remainder] string name)
            => await ReplyAsync(embed: await DataService.CreatePlaylistFromGameData(name, Context.User.Id, Context.Message));

        [Command("CreatePrivatePlaylist")]
        [Summary("Creates a private playlist of the given name. This means that only the owner"
        + " of the list can make any changes.")]
        public async Task CreatePrivatePlaylist([Remainder] string name)
            => await ReplyAsync(embed: await DataService.CreatePrivatePlaylist(name, Context.User.Id));

        [Command("AddToPlaylist")]
        [Summary("Enter a playlist and song key to add a song to a playlist.")]
        public async Task AddToPlaylist(string playlistName, [Remainder] string key)
            => await ReplyAsync(embed: await DataService.AddToPlaylist(playlistName.ToLower(), key, Context.User.Id));
            
        [Command("RemoveFromPlaylist")]
        [Summary("Removes the song you enter from the playlist.")]
        public async Task RemoveFromPlaylist(string playlistName, [Remainder] string key)
            => await ReplyAsync(embed: await DataService.RemoveFromPlaylist(playlistName.ToLower(), key, Context.User.Id));

        [Command("PrintPlaylist")]
        [Summary("Prints the content of the specified playlist.")]
        public async Task PrintPlaylist([Remainder] string playlistName)
            => await ReplyAsync(embed: await DataService.PrintPlaylist(playlistName.ToLower(), Context.Channel));

        [Command("urm")]
        [Summary("Converts playlists to the new format.")]
        public async Task UpdateRelationalMap()
            => await DataService.DBManager.animeRelationManager.UpdateRelationalMapUsingSongTable();

        [Command("PrintLists")]
        [Summary("This command will print all MAL and Anilists that the bot has saved.")]
        public async Task PrintLists()
            => await ReplyAsync(embed: await DataService.PrintAllLists());    
        
        [Command("PrintPlaylists")]
        [Summary("This command will print all playlists that the bot has saved. This command takes no arguments.")]
        public async Task PrintPlaylists()
            => await ReplyAsync(embed: await PlaylistHandler.PrintAllPlaylists(DataService.path));
    }
}

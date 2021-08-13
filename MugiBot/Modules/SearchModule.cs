using Discord.Commands;
using PartyBot.Handlers;
using PartyBot.Services;
using System.Threading.Tasks;

namespace PartyBot.Modules
{
    public class SearchModule : ModuleBase<SocketCommandContext>
    {
        public DataService DataService { get; set; }
        [Command("SearchDB")]
        [Summary("Type the name of a show or a substring of it's name in English or Romaji form and this will return" +
            "the songs that match the query. There is an optional argument of type of song i.e opening, ending, or insert. " +
            "Lastly, there is an optional argument of typing exact to specify if you want an exact match on the show")]
        public async Task SearchForSongs([Remainder] string showName)
                => await ReplyAsync(embed: await DBSearchService.SearchForShow(Context.Message, showName, "any", false));
        [Command("SearchDBExact")]
        [Summary("Type the name of a show or a substring of it's name in English or Romaji form and this will return" +
            "the songs that match the query. There is an optional argument of type of song i.e opening, ending, or insert. " +
            "Lastly, there is an optional argument of typing exact to specify if you want an exact match on the show")]
        public async Task SearchForSongsExact([Remainder] string showName)
                => await ReplyAsync(embed: await DBSearchService.SearchForShow(Context.Message, showName, "any", true));
        [Command("SearchOP")]
        [Summary("Type the name of a show or a substring of it's name in English or Romaji form and this will return" +
            "the openings that match the query.")]
        public async Task SearchOpenings([Remainder] string showName)
                => await ReplyAsync(embed: await DBSearchService.SearchForShow(Context.Message, showName, "opening", false));
        [Command("SearchOPExact")]
        [Summary("Type the exact name of a show in English or Romaji form and this will return" +
            "the openings that match the query.")]
        public async Task SearchOpeningsExact([Remainder] string showName)
                        => await ReplyAsync(embed: await DBSearchService.SearchForShow(Context.Message, showName, "opening", true));
        [Command("SearchED")]
        [Summary("Type the name of a show or a substring of it's name in English or Romaji form and this will return" +
            "the endings that match the query.")]
        public async Task SearchEndings([Remainder] string showName)
                => await ReplyAsync(embed: await DBSearchService.SearchForShow(Context.Message, showName, "ending", false));
        [Command("SearchEDExact")]
        [Summary("Type the exact name of a show in English or Romaji form and this will return" +
            "the endings that match the query.")]
        public async Task SearchEndingsExact([Remainder] string showName)
                        => await ReplyAsync(embed: await DBSearchService.SearchForShow(Context.Message, showName, "ending", true));
        [Command("SearchED")]
        [Summary("Type the name of a show or a substring of it's name in English or Romaji form and this will return" +
            "the inserts that match the query.")]
        public async Task SearchInsert([Remainder] string showName)
                => await ReplyAsync(embed: await DBSearchService.SearchForShow(Context.Message, showName, "ins", true));
        [Command("SearchEDExact")]
        [Summary("Type the exact name of a show in English or Romaji form and this will return" +
            "the inserts that match the query.")]
        public async Task SearchInsertExact([Remainder] string showName)
                        => await ReplyAsync(embed: await DBSearchService.SearchForShow(Context.Message, showName, "ins", true));
        [Command("SearchDBLinks")]
        [Summary("This functions exactly the same as SearchDB except it will print the links out as well.")]
        public async Task SearchForSongLinks(string showName, string type = "any", string exactMatch = "no")
                => await ReplyAsync(embed: await DBSearchService.SearchForShow(Context.Message, showName, type, false, "yes"));
        [Command("SearchDBLinksExact")]
        [Summary("This functions exactly the same as SearchDB except it will print the links out as well.")]
        public async Task SearchForSongLinks(string showName, string type = "any")
                => await ReplyAsync(embed: await DBSearchService.SearchForShow(Context.Message, showName, type, true, "yes"));

        [Command("SearchAuthor")]
        [Summary("Will return every song in the database by that author.")]
        public async Task SearchByAuthor([Remainder] string author)
            => await ReplyAsync(embed: await DBSearchService.SearchByAuthor(Context.Message, author, "no"));

        [Command("SearchAuthorLinks")]
        [Summary("Will return every song in the database by that author and print the links.")]
        public async Task SearchByAuthorLinks([Remainder] string author)
            => await ReplyAsync(embed: await DBSearchService.SearchByAuthor(Context.Message, author, "yes"));

        [Command("ShowStats")]
        [Summary("This will list the total stats from the database for each show that contains the string you specify.")]
        public async Task GetShowStats([Remainder] string showName)
            => await ReplyAsync(embed: await DBCalculationHandler.CalcShowStats(Context.Channel, showName, false));

        [Command("ShowStatsExact")]
        [Summary("This will list the total stats from the database for each show that exactly matches the string you specify.")]
        public async Task GetShowStatsExact([Remainder] string showName)
            => await ReplyAsync(embed: await DBCalculationHandler.CalcShowStats(Context.Channel, showName, true));

        [Command("PlayerStats")]
        [Summary("This will list the stats for the specified player on all shows that contain the string you specify." +
            " For example, !liststats dingus naruto will list dingus' stats on all songs from anime whose title contain naruto.")]
        public async Task GetPlayerStats(string playerName, string showName, string type = "any")
            => await ReplyAsync(embed: await DBSearchService.ListPlayerStats(playerName, showName, type, "no"));

        [Command("PlayerStatsExact")]
        [Summary("This will list the stats for the specified player on all shows that contain the string you specify." +
            " For example, !liststats dingus naruto will list dingus' stats on all songs from naruto.")]
        public async Task GetPlayerStatsExact(string playerName, string showName, string type = "any")
            => await ReplyAsync(embed: await DBSearchService.ListPlayerStats(playerName, showName, type, "exact"));

        [Command("PlayerStatsArtist")]
        [Summary("This will list the stats for the specified player on all shows that contain the string you specify." +
            " For example, !liststats dingus naruto will list dingus' stats on all songs from anime whose title contain naruto.")]
        public async Task GetPlayerStatsByAuthor(string playerName, string artist, string type = "any")
            => await ReplyAsync(embed: await DBSearchService.PlayerStatsByArtist(Context.Channel, playerName, artist, type, "no"));

        [Command("ListAll")]
        [Summary("This will list the stats for the specified player on all shows that contain the string you specify.")]
        public async Task ListAllPlayers(string playerName, string showName)
            => await ReplyAsync(embed: await DBSearchService.OtherPlayerStats(Context.Channel, playerName, showName, "no"));
        [Command("ListAllExact")]
        [Summary("This will list the stats for the specified player and specified show.")]
        public async Task ListAllPlayersExact(string playerName, string showName)
            => await ReplyAsync(embed: await DBSearchService.OtherPlayerStats(Context.Channel, playerName, showName, "yes"));
    }

}

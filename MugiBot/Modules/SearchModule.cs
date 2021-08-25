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
        [Command("SearchINS")]
        [Summary("Type the name of a show or a substring of it's name in English or Romaji form and this will return" +
            "the inserts that match the query.")]
        public async Task SearchInsert([Remainder] string showName)
                => await ReplyAsync(embed: await DBSearchService.SearchForShow(Context.Message, showName, "insert", true));
        [Command("SearchINSExact")]
        [Summary("Type the exact name of a show in English or Romaji form and this will return" +
            "the inserts that match the query.")]
        public async Task SearchInsertExact([Remainder] string showName)
                        => await ReplyAsync(embed: await DBSearchService.SearchForShow(Context.Message, showName, "insert", true));
        [Command("SearchDBLinks")]
        [Summary("This functions exactly the same as SearchDB except it will print the links out as well.")]
        public async Task SearchForSongLinks([Remainder] string showName)
                => await ReplyAsync(embed: await DBSearchService.SearchForShow(Context.Message, showName, "any", false, "yes"));
        [Command("SearchDBLinksExact")]
        [Summary("This functions exactly the same as SearchDB except it will print the links out as well.")]
        public async Task SearchForSongLinksExact([Remainder] string showName)
                => await ReplyAsync(embed: await DBSearchService.SearchForShow(Context.Message, showName, "any", true, "yes"));

        [Command("SearchArtist")]
        [Summary("Will return every song in the database by that author.")]
        public async Task SearchByArtist([Remainder] string author)
            => await ReplyAsync(embed: await DBSearchService.SearchByAuthor(Context.Message, author, "no"));

        [Command("SearchArtistLinks")]
        [Summary("Will return every song in the database by that author and print the links.")]
        public async Task SearchByArtistLinks([Remainder] string author)
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
        public async Task GetPlayerStats(string playerName, [Remainder] string showName)
            => await ReplyAsync(embed: await DBSearchService.ListPlayerStats(Context.Channel, playerName, showName, DataService.DBManager._rs, "any", "no"));

        [Command("PlayerStatsExact")]
        [Summary("This will list the stats for the specified player on all shows that contain the string you specify." +
            " For example, !liststats dingus naruto will list dingus' stats on all songs from naruto.")]
        public async Task GetPlayerStatsExact(string playerName, [Remainder] string showName)
            => await ReplyAsync(embed: await DBSearchService.ListPlayerStats(Context.Channel, playerName, showName, DataService.DBManager._rs, "any", "exact"));

        [Command("PlayerStatsArtist")]
        [Summary("This will list the stats for the specified player on all songs done by any artist that contains that substring." +
            " For example, !playerstatsartist neutrality Claris will print out all of neutrality's stats on songs done by artists whose name contains Claris.")]
        public async Task GetPlayerStatsByArtist(string playerName, [Remainder] string artist)
            => await ReplyAsync(embed: await DBSearchService.PlayerStatsByArtist(Context.Channel, playerName, artist));

        [Command("PlayerStatsArtistExact")]
        [Summary("This will list the stats for the specified player on all songs done by that artist." +
            " For example, !playerstatsartistexact neutrality Claris will print out all of neutrality's stats on songs done by Claris.")]
        public async Task GetPlayerStatsByArtistExact(string playerName, [Remainder] string artist)
            => await ReplyAsync(embed: await DBSearchService.PlayerStatsByArtist(Context.Channel, playerName, artist, "any", "exact"));

        [Command("ListAll")]
        [Summary("This will list the stats for the specified player on all shows that contain the string you specify." +
            " For example, !listall xm72 naruto will list out any song from naruto that xm72 has statistics for in the database")]
        public async Task ListAllPlayerStats(string playerName, [Remainder] string showName)
            => await ReplyAsync(embed: await DBSearchService.OtherPlayerStats(Context.Channel, playerName, showName, "no"));
        [Command("ListAllExact")]
        [Summary("This will list the stats for the specified player and specified show.")]
        public async Task ListAllPlayerStatsExact(string playerName, [Remainder] string showName)
            => await ReplyAsync(embed: await DBSearchService.OtherPlayerStats(Context.Channel, playerName, showName, "yes"));

        [Command("ArtistStats")]
        [Summary("This will list the stats for the specified artist on all songs done by the artist.")]
        public async Task ArtistStats([Remainder] string artist)
            => await ReplyAsync(embed: await DBSearchService.StatsByArtist(Context.Channel, artist));
    }

}

using Discord;
using Discord.Commands;
using PartyBot.Services;
using System.Threading.Tasks;


namespace PartyBot.Modules
{
    public class PlayersRulesModule : ModuleBase<SocketCommandContext>
    {
        public PlayersRulesService PlayersRulesService { get; set; }

        [Command("ListRules")]
        [Summary("Lists all rules that the bot will use to keep track of statistics. " +
            "For example, the rule Ranked will keep a separate tally of your success rates in Ranked games. Another example, is" +
            "the rule dingus enslo which will keep a separate tally for the success rates of dingus and enslo when they are both in the same lobby." +
            "This makes it very easy to see head to head statistics.")]
        public async Task ListRules()
            => await ReplyAsync(embed: await PlayersRulesService.ListRules());

        [Command("NewRule")]
        [Summary("Creates a new rule for the bot to keep track of. For example, " +
            "the rule dingus enslo which will keep a separate tally for the success rates of dingus and enslo when they are both in the same lobby." +
            "This makes it very easy to see head to head statistics.")]
        public async Task AddRule([Remainder] string rule)
            => await ReplyAsync(embed: await PlayersRulesService.NewRule(rule));

        [Command("AddPlayer")]
        [Summary("First provide an AMQ username you would like to track, "
        + "then provide a name for that AMQ user name that the databasae will use. For example,"
        + "!addplayertest mysmurf myrealaccount")]
        public async Task TrackPlayer(string key, string value)
            => await ReplyAsync(embed: await PlayersRulesService.TrackPlayer(key, value));

        [Command("RemovePlayer")]
        [Summary("This will stop tracking the data of the specified player.")]
        public async Task RemovePlayer([Remainder] string player)
            => await ReplyAsync(embed: await PlayersRulesService.RemovePlayer(player));

        [Command("ListPlayers")]
        [Summary("This will list all players that the bot is tracking the data of.")]
        public async Task ListPlayersTracked()
            => await ReplyAsync(embed: await PlayersRulesService.ListPlayersTracked());

        [Command("DeleteRule")]
        [Summary("This will delete the rule you specify and the bot will stop keeping track of data based on that rule.")]
        public async Task DeleteRule(string rule)
            => await ReplyAsync(embed: await PlayersRulesService.DeleteRule(rule));

        [Command("SetUsername")]
        [Summary("This sets your AMQ username.")]
        public async Task SetAMQUsername(string username)
            => await ReplyAsync(embed: await PlayersRulesService.SetUsername(Context.Message, username));

        [Command("RemovesUsername")]
        [Summary("This removes your AMQ username.")]
        public async Task RemovesAMQUsername()
            => await ReplyAsync(embed: await PlayersRulesService.RemoveUsername(Context.Message));
    }
}

using Discord.Commands;
using PartyBot.Services;
using System.Threading.Tasks;

namespace PartyBot.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        public HelpService HelpService { get; set; }
        public CommandService CommandService { get; set; }

        [Command("Help")]
        [Summary("Returns the summary for some command")]
        public async Task Help([Remainder] string command)
            => await ReplyAsync(embed: await HelpService.GetSummary(command, CommandService));

        [Command("Help")]
        [Summary("List all commands")]
        public async Task Help()
            => await ReplyAsync(embed: await HelpService.AllCommands(CommandService));

        [Command("TestHelp")]
        [Summary("List all commands")]
        public async Task TestHelp()
            => await ReplyAsync(embed: await HelpService.MainHelp());     

        [Command("AudioHelp")]
        [Summary("List all commands")]
        public async Task AudioHelp()
            => await ReplyAsync(embed: await HelpService.AudioHelp()); 

        [Command("TrackingHelp")]
        [Summary("List all commands")]
        public async Task TrackingHelp()
            => await ReplyAsync(embed: await HelpService.DatabaseTrackingHelp()); 

        [Command("SearchHelp")]
        [Summary("List all commands")]
        public async Task SearchHelp()
            => await ReplyAsync(embed: await HelpService.DatabaseSearchHelp()); 
        
    }
}

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
        [Summary("Returns the summary for some command.")]
        public async Task Help([Remainder] string command)
            => await ReplyAsync(embed: await HelpService.GetSummary(command, CommandService));

        [Command("Help")]
        [Summary("Presents the main help message.")]
        public async Task Help()
            => await ReplyAsync(embed: await HelpService.MainHelp());
        
        [Command("CommandsHelp")]
        [Summary("List all commands.")]
        public async Task CommandsHelp()
            => await ReplyAsync(embed: await HelpService.AllCommands(CommandService));  

        [Command("AudioHelp")]
        [Summary("Prints out helpful information for the audio commands.")]
        public async Task AudioHelp()
            => await ReplyAsync(embed: await HelpService.AudioHelp()); 

        [Command("AudioHelp2")]
        [Summary("Prints out helpful information for the audio commands.")]
        public async Task AudioHelp2()
            => await ReplyAsync(embed: await HelpService.AudioHelp());

        [Command("TrackingHelp")]
        [Summary("Prints out helpful information for the database tracking commands.")]
        public async Task TrackingHelp()
            => await ReplyAsync(embed: await HelpService.DatabaseTrackingHelp()); 

        [Command("SearchHelp")]
        [Summary("Prints out helpful information for the search commands.")]
        public async Task SearchHelp()
            => await ReplyAsync(embed: await HelpService.DatabaseSearchHelp()); 
        
        [Command("RadioHelp")]
        [Summary("Prints out helpful information for the radio.")]
        public async Task RadioHelp()
            => await ReplyAsync(embed: await HelpService.RadioHelp());
        
    }
}

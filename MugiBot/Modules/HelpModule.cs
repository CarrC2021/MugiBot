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
        public async Task Help(string command)
            => await ReplyAsync(embed: await HelpService.GetSummary(command, CommandService));

        [Command("Help")]
        [Summary("List all commands")]
        public async Task Help()
            => await ReplyAsync(embed: await HelpService.AllCommands(CommandService));
    }
}

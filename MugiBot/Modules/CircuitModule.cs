using Discord;
using Discord.Commands;
using PartyBot.Services;
using System;
using System.Threading.Tasks;

namespace PartyBot.Modules
{
    public class CircuitModule : ModuleBase<SocketCommandContext>
    {
        public PlayersRulesService PlayersRulesService { get; set; }
        public AMQCircuitService CircuitService { get; set; }

        [Command("SetTeam")]
        [Summary("Provide the team that you are a part of.")]
        public async Task SetCurrentTeam(string team)
            => await ReplyAsync(embed: await CircuitService.SetTeam(Context.Message, team));

        [Command("RemoveTeam")]
        [Summary("Removes you from the team you currently have.")]
        public async Task RemoveCurrentTeam()
            => await ReplyAsync(embed: await CircuitService.RemoveTeam(Context.Message));

        [Command("ListTeams")]
        [Summary("This will list all players that the bot is tracking the data of.")]
        public async Task ListTeams()
            => await ReplyAsync(embed: await CircuitService.ListTeamAssignments(Context.Guild));
    }
}

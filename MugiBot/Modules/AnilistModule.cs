using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PartyBot.Database;
using PartyBot.Handlers;
using PartyBot.Services;
using System.Threading.Tasks;

namespace PartyBot.Modules
{
    public class AnilistModule : ModuleBase<SocketCommandContext>
    {
        private AnilistService _anilistService = new AnilistService();
        [Command("ALUser")]
        [Summary("Gets the Anilist User associated with this username.")]
        public async Task GetUserListAsync([Remainder] string userName)
            => await ReplyAsync(embed: await _anilistService.GetUserListAsync(userName));
    }
}
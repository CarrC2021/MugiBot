using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PartyBot.Database;
using PartyBot.Handlers;
using PartyBot.Services;
using System.IO;
using System.Threading.Tasks;

namespace MugiBot.Modules
{
    public class DiscordUserModule : ModuleBase<SocketCommandContext>
    {
        [Command("SetAMQUsername")]
        [Summary("This sets your AMQ username.")]
        public async Task SetAMQUsername(string username)
            => await ReplyAsync(embed: await DiscordUserHandler.SetUsername(username, Context.User.Id));

        [Command("PrintMyInfo")]
        [Summary("Prints all the info that is associated to your discord user ID")]
        public async Task PrintMyInfo()
            => await ReplyAsync(embed: await DiscordUserHandler.PrintUserDBInformation(Context.User.Username, Context.User.Id));
    }
}
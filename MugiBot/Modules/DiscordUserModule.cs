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
        [Command("SetDBUsername")]
        [Summary("This sets what your username in the database is associated with your discord id.")]
        public async Task SetDBUsername(string username)
            => await ReplyAsync(embed: await DiscordUserHandler.SetUserDatabaseName(Context.User.Id, username));

        [Command("PrintMyInfo")]
        [Summary("Prints all the info that is associated to your discord user ID")]
        public async Task PrintMyInfo()
            => await ReplyAsync(embed: await DiscordUserHandler.PrintUserDBInformation(Context.User.Username, Context.User.Id));
    }
}
using Discord.Commands;
using PartyBot.Handlers;
using PartyBot.Services;
using System.Threading.Tasks;

namespace PartyBot.Modules
{
    public class AnilistModule : ModuleBase<SocketCommandContext>
    {
        public AnilistService AnilistService = new AnilistService(GlobalData.Config.RootFolderPath);

        [Command("SetAL")]
        [Summary("Sets the discord user's anilist to the string provided. Make sure to copy "
        + "and paste the exact user name. Use the command like this !setal ARealDingus.")]
        public async Task SetUserListAsync([Remainder] string anilistName)
            => await ReplyAsync(embed: await DiscordUserHandler.SetUserListNameAsync(Context.User.Id, "anilist", anilistName));

        [Command("UpdateAL")]
        [Summary("Updates the file containing the user's anilist.")]
        public async Task UpdateUserListAsync()
            => await ReplyAsync(embed: await AnilistService.GetUserListAsync(Context.User.Id));
    }
}
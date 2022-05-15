using Discord.Commands;
using PartyBot.Handlers;
using PartyBot.Services;
using System.Threading.Tasks;

namespace PartyBot.Modules
{
    public class UserListModule : ModuleBase<SocketCommandContext>
    {
        public AnilistService AnilistService = new AnilistService(GlobalData.Config.RootFolderPath);

        [Command("SetAL")]
        [Summary("Sets the discord user's anilist to the string provided. Make sure to copy "
        + "and paste the exact user name. Use the command like this !setal ARealDingus.")]
        public async Task SetUserListAsync([Remainder] string anilistName)
            => await ReplyAsync(embed: await DiscordUserHandler.SetUserListNameAsync(Context.User.Id, "anilist", anilistName));

        [Command("RemoveAL")]
        [Summary("Removes the discord user's current anilist.")]
        public async Task RemoveUserListAsync()
            => await ReplyAsync(embed: await DiscordUserHandler.RemoveUserListNameAsync(Context.User.Id, "anilist"));

        [Command("SetMAL")]
        [Summary("Sets the discord user's MyAnimeList to the string provided. Make sure to copy "
        + "and paste the exact user name. Use the command like this !setmal ARealDingus.")]
        public async Task SetMalUserListAsync([Remainder] string malName)
            => await ReplyAsync(embed: await DiscordUserHandler.SetUserListNameAsync(Context.User.Id, "mal", malName));

        [Command("RemoveMAL")]
        [Summary("Removes the discord user's current MyAnimeList.")]
        public async Task RemoveMALUserListAsync()
            => await ReplyAsync(embed: await DiscordUserHandler.RemoveUserListNameAsync(Context.User.Id, "mal"));

        [Command("UpdateAL")]
        [Summary("Updates the file containing the user's anilist.")]
        public async Task UpdateUserListAsync()
            => await ReplyAsync(embed: await AnilistService.GetUserListAsync(Context.User.Id));

        [Command("UpdateMAL")]
        [Summary("Updates the file containing the user's MAL.")]
        public async Task UpdateMALUserListAsync()
            => await ReplyAsync(embed: await MALHandler.UpdateUserListAsync(Context.User.Id));
    }
}
using System;
using Discord;
using PartyBot.Database;
using PartyBot.Services;
using System.Threading.Tasks;
using System.Text;

namespace PartyBot.Handlers
{
    public static class DiscordUserHandler
    {
        public static async Task AddUserToDBAsync(ulong id, string DatabaseName = null)
        {
            using var db = new AMQDBContext();
            try
            {
                await db.DiscordUsers.AddAsync(new DiscordUser(id, DatabaseName));
            }
            catch(Exception ex)
            {
                await LoggingService.LogCriticalAsync(ex.Source, ex.Message + ex.StackTrace, ex);
            }
            await db.SaveChangesAsync();
        }

        public static async Task<Embed> SetUserDatabaseName(ulong id, string DatabaseName)
        {
            using var db = new AMQDBContext();
            var user = await db.DiscordUsers.FindAsync(id);
            if (user == null)
                await db.AddAsync(new DiscordUser(id, DatabaseName));
            else
                user.DatabaseName = DatabaseName;
            await db.SaveChangesAsync();
            return await EmbedHandler.CreateBasicEmbed("Database", $"Set your database name to {DatabaseName}",  Color.DarkPurple);
        }

        public static async Task<Embed> SetUserListNameAsync(ulong id, string listPlatform, string listName)
        {
            using var db = new AMQDBContext();
            var user = await db.DiscordUsers.FindAsync(id);
            if (user == null)
                await DiscordUserHandler.AddUserToDBAsync(id);
            user = await db.DiscordUsers.FindAsync(id);
            // Very ugly but works for now, sets the list platform correctly.
            if (listPlatform.Equals("anilist"))
                user.AnilistName = listName;
            if (listPlatform.Equals("mal"))
                user.MALName = listName;
            if (listPlatform.Equals("kitsu"))
                user.KitsuName = listName;
            await db.SaveChangesAsync();
            return await EmbedHandler.CreateBasicEmbed("Anilist", $"Set your {listPlatform} to {listName}",  Color.DarkPurple);
        }

         public static async Task<Embed> RemoveUserListNameAsync(ulong id, string listPlatform)
        {
            using var db = new AMQDBContext();
            var user = await db.DiscordUsers.FindAsync(id);
            if (user == null)
                await DiscordUserHandler.AddUserToDBAsync(id);
            user = await db.DiscordUsers.FindAsync(id);
            // Very ugly but works for now, sets the list platform correctly.
            if (listPlatform.Equals("anilist"))
                user.AnilistName = null;
            if (listPlatform.Equals("mal"))
                user.MALName = null;
            if (listPlatform.Equals("kitsu"))
                user.KitsuName = null;
            await db.SaveChangesAsync();
            return await EmbedHandler.CreateBasicEmbed("Anilist", $"Removed your {listPlatform}",  Color.DarkPurple);
        }

        public static async Task<Embed> PrintUserDBInformation(string discordUserName, ulong id)
        {
            using var db = new AMQDBContext();
            var userData = await db.DiscordUsers.FindAsync(id);
            if (userData == null)
            {
                await db.DiscordUsers.AddAsync(new DiscordUser(id));
                await db.SaveChangesAsync();
                return await EmbedHandler.CreateBasicEmbed("Discord User Data", $"Found nothing for you in the database, "
                + "created an entry for you. Use !setal to set your anilist or !setamqusername to set your username.", Color.Blue);
            }
            var sb = new StringBuilder($"Database and Anilist Information for {discordUserName}:\n\n");
            if (userData.DatabaseName != null)
                sb.Append($"Database username is currently set to {userData.DatabaseName}\n\n");
            if (userData.AnilistName != null)
                sb.Append($"Anilist username is currently set to {userData.AnilistName}\n\n");
            if (userData.MALName != null)
                sb.Append($"MyAnimeList username is currently set to {userData.MALName}\n\n");

            return await EmbedHandler.CreateBasicEmbed("Discord User Data", sb.ToString(), Color.DarkPurple);
        }
    }
}
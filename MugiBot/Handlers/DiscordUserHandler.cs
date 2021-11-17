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

        public static async Task<Embed> SetUsername(string username, ulong id)
        {
            using var db = new AMQDBContext();
            var userData = await db.DiscordUsers.FindAsync(id);
            if (userData == null)
            {
                await db.DiscordUsers.AddAsync(new DiscordUser(id, username));
                await db.SaveChangesAsync();
                return await EmbedHandler.CreateBasicEmbed("Data", $"Your AMQ username is now set to {username}.", Color.DarkPurple);
            }
            userData.DatabaseName = username;
            await db.SaveChangesAsync();
            return await EmbedHandler.CreateBasicEmbed("Data", $"Your AMQ username is now set to {username}.", Color.DarkPurple);
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
            var sb = new StringBuilder($"AMQ and Anilist Information for {discordUserName}:\n\n");
            if (userData.DatabaseName != null)
                sb.Append($"AMQ username has been set to {userData.DatabaseName}\n\n");
            if (userData.AnilistName != null)
                sb.Append($"Anilist username has been set to {userData.AnilistName}\n\n");

            return await EmbedHandler.CreateBasicEmbed("Discord User Data", sb.ToString(), Color.DarkPurple);
        }
    }
}
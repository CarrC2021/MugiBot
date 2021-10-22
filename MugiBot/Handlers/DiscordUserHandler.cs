using System;
using Discord;
using PartyBot.Database;
using PartyBot.Services;
using System.Threading.Tasks;

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
            return await EmbedHandler.CreateBasicEmbed("Database", $"Set your database name to {DatabaseName}", Color.Blue);
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
            return await EmbedHandler.CreateBasicEmbed("Anilist", $"Set your {listPlatform} to {listName}", Color.Blue);
        }
    }
}
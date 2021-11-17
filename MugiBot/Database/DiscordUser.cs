using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartyBot.Database
{
    /// <summary>
    /// DiscordUser is a class which keeps track of data that is meant to be stored in a table in the SQL database.
    /// </summary>
    [Table("DiscordUser")]
    public class DiscordUser
    {
        [Key]
        public ulong ID { get; set; }
        public string DatabaseName { get; set; }
        public string AnilistName { get; set; }
        public string KitsuName { get; set; }
        public string MALName { get; set; }
        
        public DiscordUser()
        {

        }
        public DiscordUser(ulong id)
        {
            ID = id;
        }
        public DiscordUser(ulong id, string dbName)
        {
            ID = id;
            DatabaseName = dbName;
        }
        public DiscordUser(ulong id, string dbName, string animeService, string listName)
        {
            ID = id;
            DatabaseName = dbName;
            if (animeService.Equals("anilist"))
                AnilistName = listName;
            else if (animeService.Equals("mal"))
                MALName = listName;
            else
                KitsuName = listName;
        }


    }
}

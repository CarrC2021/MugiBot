using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartyBot.Database
{
    /// <summary>
    /// DiscordUser is a class which keeps track of data that is meant to be stored in a table in the SQL database.
    /// </summary>
    [Table("AnimeRelationalMaps")]
    public class AnimeRelationalMap
    {
        [Key]
        public int AnnID { get; set; }
        public string EngName { get; set; }
        public string Romaji { get; set; }
        public int AnilistID { get; set; }
        public int KitsuID { get; set; }
        public int MALID { get; set; }
        public string MalLink { get; set; }

        public AnimeRelationalMap()
        {

        }
        public AnimeRelationalMap(int annID)
        {
            AnnID = annID;
        }
        public AnimeRelationalMap(int annID, string engName, string romaji)
        {
            AnnID = annID;
            EngName = engName;
            Romaji = romaji;
        }
        public AnimeRelationalMap(int annID, string engName, string romaji, int anilistID, int kitsuID, int mID)
        {
            AnnID = annID;
            EngName = engName;
            Romaji = romaji;
            AnilistID = anilistID;
            KitsuID = kitsuID;
            MALID = mID;
        }

        public string PrintRelations()
        {
            return $"English name: {EngName} and Romaji name: {Romaji}\nAnnID: {AnnID} \nAnilistID: {AnilistID} \nMalID: {MALID} \nKitsuID: {KitsuID}";
        }
    }
}
using Discord;
using Microsoft.EntityFrameworkCore;
using PartyBot.Database;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PartyBot.Handlers
{
    public class DBMergeHandler
    {

        // Merges player objects into a new PlayerTableObject with a new player name.
        public static async Task<Embed> MergePlayers(AMQDBContext _db, string nameToFind, string nameToMergeTo)
        {
            var Query = await _db.PlayerStats
                .AsNoTracking()
                .Where(k => k.PlayerName.Equals(nameToFind))
                .ToListAsync();
            foreach (PlayerTableObject tableObject in Query)
            {
                var dbEntry = await _db.PlayerStats.FindAsync(tableObject.Key);
                string newKey = PlayerTableObject.MakePlayerTableKey(tableObject.AnnID,
                 tableObject.Type, tableObject.SongName, tableObject.Artist, nameToMergeTo, tableObject.Rule);
                var result = await _db.PlayerStats.FindAsync(newKey);
                if (result == null)
                {
                    Console.WriteLine(tableObject.Key);
                    var tempPlayer = new PlayerTableObject(tableObject, nameToMergeTo);
                    await _db.PlayerStats.AddAsync(tempPlayer);
                    _db.PlayerStats.Remove(dbEntry);
                }
                else
                {
                    result.TotalTimesPlayed += dbEntry.TotalTimesPlayed;
                    result.TimesCorrect += dbEntry.TimesCorrect;
                    _db.PlayerStats.Remove(dbEntry);
                    Console.WriteLine($"incrementing {result.TotalTimesPlayed} by {dbEntry.TotalTimesPlayed} for {dbEntry.Key}");
                }
            }
            await _db.SaveChangesAsync();
            return await EmbedHandler.CreateBasicEmbed("Data", "I hope that worked", Color.Blue);
        }
    }

}
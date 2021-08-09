using Microsoft.EntityFrameworkCore;
using PartyBot.Database;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyBot.Handlers
{

    public class SearchHandler
    {
        public static async Task<SongTableObject> UseSongKey(AMQDBContext _db, string key)
        {
            SongTableObject song = await _db.SongTableObject.FindAsync(key);
            if (song != null)
            {
                return song;
            }
            // This will catch any weird cases where the casing has been changed in the game.
            List<SongTableObject> result = await _db.SongTableObject
                .AsNoTracking()
                .Where(x => x.Key.ToLower().Equals(key.ToLower()))
                .ToListAsync();
            return result[0];
        }

        public static async Task<List<PlayerTableObject>> AllObjectsForPlayer(AMQDBContext _db, string name, bool onlyFromList = true)
        {
            List<PlayerTableObject> Query = await _db.PlayerStats
                   .AsNoTracking()
                   .Where(f => f.Rule.Equals(""))
                   .Where(j => j.PlayerName.ToLower().Equals(name.ToLower()))
                   .ToListAsync();
            if (onlyFromList)
                Query = Query.Where(y => y.FromList > 0).ToList();
            return Query;
        }

        // Need to split this function up like was done below.
        public static async Task<List<PlayerTableObject>> PlayerStatsSearch(AMQDBContext _db, string playerName, string showName, string type, string exactMatch = "no")
        {
            List<PlayerTableObject> Shows;
            List<PlayerTableObject> Romajis;
            if (type.Equals("any"))
            {
                if (exactMatch.ToLower().Equals("exact") || exactMatch.ToLower().Equals("exactmatch"))
                {
                    //This will return player table objects where the showName is an exact match
                    Shows = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.Show.ToLower().Equals(showName.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();

                    Romajis = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.Romaji.ToLower().Equals(showName.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();
                }
                else
                {
                    Shows = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.Show.ToLower().Contains(showName.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();

                    Romajis = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.Romaji.ToLower().Contains(showName.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();
                }

            }
            else
            {
                if (exactMatch.ToLower().Equals("exact") || exactMatch.ToLower().Equals("exactmatch"))
                {
                    //this will return player table objects where the showname is an exact match
                    Shows = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.Show.ToLower().Equals(showName.ToLower()))
                        .Where(j => j.Type.ToLower().Contains(type.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();

                    Romajis = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.Romaji.ToLower().Equals(showName.ToLower()))
                        .Where(j => j.Type.ToLower().Contains(type.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();
                }
                else
                {
                    Shows = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.Show.ToLower().Contains(showName.ToLower()))
                        .Where(j => j.Type.ToLower().Contains(type.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();

                    Romajis = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.Romaji.ToLower().Contains(showName.ToLower()))
                        .Where(j => j.Type.ToLower().Contains(type.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();
                }
            }

            return Shows.Union(Romajis, new PlayerTableObjectComparer()).ToList();
        }
        public static async Task<List<SongTableObject>> SearchAuthor(AMQDBContext _db, string author)
        {
            List<SongTableObject> Songs;
            Songs = await _db.SongTableObject
                .AsNoTracking()
                .Where(x => x.Artist.ToLower().Contains(author.ToLower()))
                .ToListAsync();

            return Songs;
        }

        /// <summary>
        /// Searches the database for any show in the database whose name matches the string given by the parameter
        /// <param name="name"/>. Additionally, if the parameter <param name="type"/> is not equal to "any"
        /// then it will restrict the search to shows whose type contains this argument.
        /// <summary>
        public static async Task<List<SongTableObject>> ExactShowSearch(AMQDBContext _db, string name, string type)
        {
            List<SongTableObject> Shows = await _db.SongTableObject
                        .AsNoTracking()
                        .Where(x => x.Show.ToLower().Equals(name.ToLower()))
                        .ToListAsync();

            List<SongTableObject> Romajis = await _db.SongTableObject
                        .AsNoTracking()
                        .Where(x => x.Romaji.ToLower().Equals(name.ToLower()))
                        .ToListAsync();
            // If a type of show has been specified remove any song that does not match the specification.            
            if (!type.Equals("any"))
            {
                Shows = Shows.Where(k => k.Type.ToLower().Contains(type.ToLower())).ToList();
                Romajis = Romajis.Where(k => k.Type.ToLower().Contains(type.ToLower())).ToList();
            }

            return Shows.Union(Romajis, new SongTableObjectComparer()).ToList();
        }

        /// <summary>
        /// Searches the database for any show in the database whose name contains the substring
        /// <param name="name"/>. Additionally, if the parameter <param name="type"/> is not equal to "any"
        /// then it will restrict the search to shows whose type contains this argument.
        /// <summary>
        public static async Task<List<SongTableObject>> ContainsShowSearch(AMQDBContext _db, string name, string type) 
        {
            List<SongTableObject> Shows = await _db.SongTableObject
                        .AsNoTracking()
                        .Where(x => x.Show.ToLower().Contains(name.ToLower()))
                        .ToListAsync();

            List<SongTableObject> Romajis = await _db.SongTableObject
                        .AsNoTracking()
                        .Where(x => x.Romaji.ToLower().Contains(name.ToLower()))
                        .ToListAsync();
            // If a type of show has been specified remove any song that does not match the specification.
            if (!type.Equals("any"))
            {
                Shows = Shows.Where(k => k.Type.ToLower().Contains(type.ToLower())).ToList();
                Romajis = Romajis.Where(k => k.Type.ToLower().Contains(type.ToLower())).ToList();
            }
            return Shows.Union(Romajis, new SongTableObjectComparer()).ToList();
        }

        public static async Task<List<SongTableObject>> ShowSearch(AMQDBContext _db, string name, string type, bool exactMatch)
        {
            if (exactMatch)
                return await ExactShowSearch(_db, name, type);
            return await ContainsShowSearch(_db, name, type);
        }
    }
}


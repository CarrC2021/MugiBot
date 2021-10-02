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
                return song;
            return null;
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

        public static async Task<List<PlayerTableObject>> ExactPlayerStatsSearch(AMQDBContext _db, string playerName, string showName, string type)
        {
            List<PlayerTableObject> Shows = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.Show.ToLower().Equals(showName.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();

            List<PlayerTableObject> Romajis = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.Romaji.ToLower().Equals(showName.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();
            if (!type.Equals("any"))
            {
                Shows = Shows.Where(j => j.Type.ToLower().Contains(type.ToLower())).ToList();
                Romajis = Romajis.Where(j => j.Type.ToLower().Contains(type.ToLower())).ToList();
            }
            return Shows.Union(Romajis, new PlayerTableObjectComparer()).ToList();
        }
        // Finds PlayerTableObjects whose showname contains the substring specified and meets the other conditions.
        public static async Task<List<PlayerTableObject>> ContainsPlayerStatsSearch(AMQDBContext _db, string playerName, string showName, string type)
        {
            List<PlayerTableObject> Shows = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.Show.ToLower().Contains(showName.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();

            List<PlayerTableObject> Romajis = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.Romaji.ToLower().Contains(showName.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();
            if (!type.Equals("any"))
            {
                Shows = Shows.Where(j => j.Type.ToLower().Contains(type.ToLower())).ToList();
                Romajis = Romajis.Where(j => j.Type.ToLower().Contains(type.ToLower())).ToList();
            }
            return Shows.Union(Romajis, new PlayerTableObjectComparer()).ToList();
        }

        // Calls helper functions to perform a database query on the PlayerStats table.
        public static async Task<List<PlayerTableObject>> PlayerStatsSearch(AMQDBContext _db, string playerName, string showName, string type, string exactMatch = "no")
        {
            if (exactMatch.ToLower().Equals("exact") || exactMatch.ToLower().Equals("exactmatch"))
                return await ExactPlayerStatsSearch(_db, playerName, showName, type);
            
            return await ContainsPlayerStatsSearch(_db, playerName, showName, type);
        }

        public static async Task<List<PlayerTableObject>> ExactArtistStatsSearch(AMQDBContext _db, string playerName, string artist, string type)
        {
            List<PlayerTableObject> Shows = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.Artist.ToLower().Equals(artist.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();
            return Shows;
        }


        // Finds PlayerTableObjects whose Artist contains the substring specified and meets the other conditions.
        public static async Task<List<PlayerTableObject>> ContainsArtistStatsSearch(AMQDBContext _db, string playerName, string artist, string type)
        {
            List<PlayerTableObject> Shows = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.Artist.ToLower().Contains(artist.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();
            return Shows;
        }

        // Calls helper functions to perform a database query on the PlayerStats table.
        public static async Task<List<PlayerTableObject>> PlayerStatsSearchByArtist(AMQDBContext _db, string playerName, string artist, string type = "any", string exactMatch = "no")
        {
            if (exactMatch.ToLower().Equals("exact") || exactMatch.ToLower().Equals("exactmatch"))
                return await ExactArtistStatsSearch(_db, playerName, artist, type);
            
            return await ContainsArtistStatsSearch(_db, playerName, artist, type);
        }

        public static async Task<List<SongTableObject>> SearchAuthor(AMQDBContext _db, string author, bool exact)
        {
            List<SongTableObject> Songs;
            Songs = await _db.SongTableObject
                .AsNoTracking()
                .Where(x => x.Artist.ToLower().Contains(author.ToLower()))
                .ToListAsync();
            if (exact)
                Songs.Where(x =>  x.Artist.ToLower().Equals(author.ToLower()));

            return Songs;
        }

        public static async Task<List<PlayerTableObject>> StatsSearchArtistContains(AMQDBContext _db, string author, string type)
        {
            List<PlayerTableObject> Songs;
            Songs = await _db.PlayerStats
                .AsNoTracking()
                .Where(x => x.Artist.ToLower().Contains(author.ToLower()))
                .Where(x => x.Rule.Equals(""))
                .ToListAsync();

            return Songs;
        }

        
        public static async Task<List<PlayerTableObject>> StatsSearchArtistExact(AMQDBContext _db, string artist, string type)
        {
            List<PlayerTableObject> Songs;
            Songs = await _db.PlayerStats
                .AsNoTracking()
                .Where(x => x.Artist.ToLower().Equals(artist.ToLower()))
                .Where(x => x.Rule.Equals(""))
                .ToListAsync();

            return Songs;
        }

        public static async Task<List<PlayerTableObject>> StatsSearchArtist(AMQDBContext _db, string artist, string type = "any", bool exact = false)
        {
            if (exact)
                return await StatsSearchArtistExact(_db, artist, type);
            return await StatsSearchArtistContains(_db, artist, type);
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


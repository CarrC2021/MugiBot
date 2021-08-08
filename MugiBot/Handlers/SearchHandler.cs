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
            var song = await _db.FindAsync<SongTableObject>(key.ToLower());
            if (song != null)
            {
                return song;
            }

            var result = await _db.SongTableObject
                .AsNoTracking()
                .Where(x => x.Key.ToLower().Equals(key.ToLower()))
                .ToListAsync();
            return result[0];
        }

        public static async Task<List<PlayerTableObject>> AllObjectsForPlayer(AMQDBContext _db, string name, bool onlyFromList = true)
        {
            if (onlyFromList)
            {
                var Query = await _db.PlayerStats
                   .AsNoTracking()
                   .Where(f => f.Rule.Equals(""))
                   .Where(j => j.PlayerName.ToLower().Equals(name.ToLower()))
                   .Where(k => k.FromList > 0)
                   .ToListAsync();
                return Query;
            }
            var PlayerQuery = await _db.PlayerStats
                   .AsNoTracking()
                   .Where(f => f.Rule.Equals(""))
                   .Where(j => j.PlayerName.ToLower().Equals(name.ToLower()))
                   .ToListAsync();
            return PlayerQuery;
        }

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
                        .Where(x => x.SongObject.Show.ToLower().Equals(showName.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();

                    Romajis = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.SongObject.Romaji.ToLower().Equals(showName.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();
                }
                else
                {
                    Shows = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.SongObject.Show.ToLower().Contains(showName.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();

                    Romajis = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.SongObject.Romaji.ToLower().Contains(showName.ToLower()))
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
                        .Where(x => x.SongObject.Show.ToLower().Equals(showName.ToLower()))
                        .Where(j => j.SongObject.Type.ToLower().Contains(type.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();

                    Romajis = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.SongObject.Romaji.ToLower().Equals(showName.ToLower()))
                        .Where(j => j.SongObject.Type.ToLower().Contains(type.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();
                }
                else
                {
                    Shows = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.SongObject.Show.ToLower().Contains(showName.ToLower()))
                        .Where(j => j.SongObject.Type.ToLower().Contains(type.ToLower()))
                        .Where(j => j.Rule.Equals(""))
                        .ToListAsync();

                    Romajis = await _db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.ToLower().Equals(playerName.ToLower()))
                        .Where(x => x.SongObject.Romaji.ToLower().Contains(showName.ToLower()))
                        .Where(j => j.SongObject.Type.ToLower().Contains(type.ToLower()))
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
        public static async Task<List<SongTableObject>> ShowSearch(AMQDBContext _db, string name, string type, bool exactMatch)
        {
            List<SongTableObject> Shows;
            List<SongTableObject> Romajis;
            if (type.Equals("any"))
            {
                if (exactMatch)
                {
                    Shows = await _db.SongTableObject
                        .AsNoTracking()
                        .Where(x => x.Show.ToLower().Equals(name.ToLower()))
                        .ToListAsync();

                    Romajis = await _db.SongTableObject
                        .AsNoTracking()
                        .Where(x => x.Romaji.ToLower().Equals(name.ToLower()))
                        .ToListAsync();
                }
                else
                {
                    Shows = await _db.SongTableObject
                        .AsNoTracking()
                        .Where(x => x.Show.ToLower().Contains(name.ToLower()))
                        .ToListAsync();

                    Romajis = await _db.SongTableObject
                        .AsNoTracking()
                        .Where(x => x.Romaji.ToLower().Contains(name.ToLower()))
                        .ToListAsync();
                }
            }
            else
            {
                if (exactMatch)
                {
                    Shows = await _db.SongTableObject
                        .AsNoTracking()
                        .Where(k => k.Type.ToLower().Contains(type.ToLower()))
                        .Where(x => x.Show.ToLower().Equals(name.ToLower()))
                        .ToListAsync();

                    Romajis = await _db.SongTableObject
                        .AsNoTracking()
                        .Where(k => k.Type.ToLower().Contains(type.ToLower()))
                        .Where(x => x.Romaji.ToLower().Equals(name.ToLower()))
                        .ToListAsync();
                }
                else
                {
                    Shows = await _db.SongTableObject
                        .AsNoTracking()
                        .Where(k => k.Type.ToLower().Contains(type.ToLower()))
                        .Where(x => x.Show.ToLower().Contains(name.ToLower()))
                        .ToListAsync();

                    Romajis = await _db.SongTableObject
                        .AsNoTracking()
                        .Where(k => k.Type.ToLower().Contains(type.ToLower()))
                        .Where(x => x.Romaji.ToLower().Contains(name.ToLower()))
                        .ToListAsync();
                }
            }

            return Shows.Union(Romajis, new SongTableObjectComparer()).ToList();
        }
    }
}


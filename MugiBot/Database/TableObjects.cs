using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PartyBot.Database
{
    public class SongTableObject
    {
        [Key]
        //This will be the songAnnId
        public int Key { get; set; }
        public string SongName { get; set; }
        public string Artist { get; set; }
        public string Type { get; set; }
        public string Show { get; set; }
        public string Romaji { get; set; }
        public string MP3 { get; set; }
        public string _720 { get; set; }
        public string _480 { get; set; }
        public int AnnID { get; set; }

        public SongTableObject()
        {
            SongName = "";
            Artist = "";
            Type = "";
            Show = "";
            MP3 = "";
            _720 = "";
            _480 = "";
            AnnID = 0;
            Key = 700000000;
        }
        public SongTableObject(string song, string art, string t, string Showname, string Roma, string u, string v, int songAnnId)
        {
            SongName = song;
            Artist = art;
            Type = t;
            Show = Showname;
            Romaji = Roma;
            MP3 = u;
            _720 = v;
            Key = songAnnId;
            AnnID = 0;
        }
        public SongTableObject(string song, string art, string t, string Showname, string Roma, string u, int Id, string _720link, int songAnnId)
        {
            SongName = song;
            Artist = art;
            Type = t;
            Show = Showname;
            Romaji = Roma;
            MP3 = u;
            _720 = _720link;
            Key = songAnnId;
            AnnID = Id;
        }
        public SongTableObject(string song, string art, string t, string Showname, string Roma, string u, int Id, string _720link, string _480link, int songAnnId)
        {
            SongName = song;
            Artist = art;
            Type = t;
            Show = Showname;
            Romaji = Roma;
            MP3 = u;
            _720 = _720link;
            _480 = _480link;
            Key = songAnnId;
            AnnID = Id;
        }
        public static string MakeSongTableKey(string showname, string songtype, string songname, string artist)
        {
            try
            {
                string key = showname.ToLower() + " " + songtype.ToLower() + " " + songname.ToLower() + " by " + artist.ToLower();
                return key;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex.StackTrace, ex.Source);
                return "Remove From Database";
            }
        }

        public static string MakeSongTableKeyFromPlayer(PlayerTableObject pt)
        {
            try
            {
                string key = pt.Show.ToLower() + " " + pt.Type.ToLower() + " " + pt.SongName.ToLower() + " by " + pt.Artist.ToLower();
                return key;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex.StackTrace, ex.Source);
                return "Remove From Database";
            }
        }

        public static string PrintSongTableObject(SongTableObject tableObject)
        {
            return $"{tableObject.Show} {tableObject.Type} {tableObject.SongName} by {tableObject.Artist}";
        }
    }

    public class PlayerTableObject
    {
        [Key]
        public string Key { get; set; }
        public string PlayerName { get; set; }
        public int TotalTimesPlayed { get; set; }
        public string Artist { get; set; }
        public int TimesCorrect { get; set; }
        public int FromList { get; set; }
        public string Type { get; set; }
        public string Show { get; set; }
        public string SongName { get; set; }
        public string Romaji { get; set; }
        public string Rule { get; set; }
        public int AnnID { get; set; }

        public PlayerTableObject()
        {
            Key = "";
            PlayerName = "";
            TotalTimesPlayed = 0;
            TimesCorrect = 0;
            FromList = 0;
        }
        public PlayerTableObject(string showname, string roma, string SongName, string t, string artist, string player, int list, string rule, int annId)
        {
            Show = showname;
            Romaji = roma;
            Type = t;
            PlayerName = player;
            TotalTimesPlayed = 0;
            TimesCorrect = 0;
            FromList = list;
            Rule = rule;
            Artist = artist;
            AnnID = annId;
            Key = MakePlayerTableKey(AnnID, Type, SongName, artist, PlayerName, Rule);
        }
        public PlayerTableObject(string showname, string roma, string songName, string t, string player, string artist, int list, bool correct, string rule, int annId)
        {
            Show = showname;
            Romaji = roma;
            Type = t;
            PlayerName = player;
            SongName = songName;
            Artist = artist;
            TotalTimesPlayed = 1;
            if (correct)
            {
                TimesCorrect = 1;
            }
            else
            {
                TimesCorrect = 0;
            }
            FromList = list;
            Rule = rule;
            AnnID = annId;
            Key = MakePlayerTableKey(AnnID, Type, SongName, artist, PlayerName, Rule);
        }
        public PlayerTableObject(PlayerTableObject tableObject, string newName)
        {
            Show = tableObject.Show;
            Romaji = tableObject.Romaji;
            Type = tableObject.Type;
            SongName = tableObject.SongName;
            PlayerName = newName;
            TotalTimesPlayed = tableObject.TotalTimesPlayed;
            TimesCorrect = tableObject.TimesCorrect;
            FromList = tableObject.FromList;
            Rule = tableObject.Rule;
            Artist = tableObject.Artist;
            Key = MakePlayerTableKey(AnnID, Type, SongName, Artist, PlayerName, Rule);
        }
        public void Update(bool correct, Dictionary<string, int> dict)
        {
            dict.TryGetValue(PlayerName, out var curr);
            FromList = curr;
            if (correct)
                TimesCorrect += 1;
            TotalTimesPlayed += 1;
        }
        public void Increment(bool correct)
        {
            if (correct)
                TimesCorrect += 1;
            TotalTimesPlayed += 1;
        }
        public static string MakePlayerTableKey(int annId, string songtype, string songname, string artist, string playername, string rule)
        {
            return annId + " " + songtype.ToLower() + " " +
                songname.ToLower() + " by " + artist.ToLower() + " " + playername.ToLower() + " " + rule.ToLower();
        }

        public static PlayerTableObject ConvertSongToPlayerTable(SongData songData, string player, int list, string rule, bool correct)
        {
            return new PlayerTableObject(songData.anime.english, songData.anime.romaji,
                songData.name, songData.type, player, songData.artist, list, correct, rule, songData.annId);
        }
    }


    public class SongTableObjectComparer : IEqualityComparer<SongTableObject>
    {
        public bool Equals(SongTableObject x, SongTableObject y)
        {
            if (x.Key.Equals(y.Key))
            {
                return true;
            }
            return false;
        }
        public int GetHashCode(SongTableObject obj)
        {
            return obj.Key.GetHashCode();
        }
    }

    public class PlayerTableObjectComparer : IEqualityComparer<PlayerTableObject>
    {
        public bool Equals(PlayerTableObject x, PlayerTableObject y)
        {
            if (x.Key.Equals(y.Key))
            {
                return true;
            }
            return false;
        }
        public int GetHashCode(PlayerTableObject obj)
        {
            return obj.Key.GetHashCode();
        }
    }
}

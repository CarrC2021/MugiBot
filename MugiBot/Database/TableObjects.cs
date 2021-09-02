using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PartyBot.DataStructs;

namespace PartyBot.Database
{
    /// <summary>
    /// SongTableObject is a class which represents a single song stored in the sql database.
    /// </summary>
    public class SongTableObject
    {
        [Key]
        public string Key { get; set; }
        public string SongName { get; set; }
        public string Artist { get; set; }
        public string Type { get; set; }
        public string Show { get; set; }
        public string Romaji { get; set; }
        public string MP3 { get; set; }
        public string _720 { get; set; }
        public string _480 { get; set; }
        public int AnnID { get; set; }
        public int AnnSongID { get; set; }

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
            Key = "";
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

            Key = MakeSongTableKey(0, t, song, art);
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
            Key = MakeSongTableKey(Id, t, song, art);
            AnnID = Id;
        }
        public SongTableObject(string song, string art, string t, string Showname, string Roma, string u, int Id, string _720link, string _480link, int annSongId)
        {
            SongName = song;
            Artist = art;
            Type = t;
            Show = Showname;
            Romaji = Roma;
            MP3 = u;
            _720 = _720link;
            _480 = _480link;
            Key = MakeSongTableKey(Id, t, song, art);
            AnnID = Id;
            AnnSongID = annSongId;
        }
        /// <summary>
        /// Initializes a new instance of a SongTableObject from a <see cref="PartyBot.DataStructs.SongData"> parameter.
        /// </summary>
        public static SongTableObject SongDataToSongTableObject(SongData data)
        {
            return new SongTableObject(data.name, data.artist, data.type, data.anime.english,
             data.anime.romaji, data.urls.catbox._0, data.annId, data.urls.catbox._720, data.urls.catbox._480, -1);
        }
        public static SongTableObject SongListDataToTable(SongListData data)
        {
            return new SongTableObject(data.songName, data.artist, data.type, data.animeEng,
             data.animeRomaji, data.LinkMp3, data.annId, data.LinkVideo, -1);
        }
        public static string MakeSongTableKey(int AnnID, string songtype, string songname, string artist)
        {
            try
            {
                string key = $"{AnnID} {songtype.ToLower()} {songname.ToLower()} by {artist.ToLower()}";
                return key;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex.StackTrace, ex.Source);
                return "Remove From Database";
            }
        }

        public static string MakeSongTableKey(PlayerTableObject pt)
        {
            try
            {
                string key = $"{pt.AnnID} {pt.Type.ToLower()} {pt.SongName.ToLower()} by {pt.Artist.ToLower()}";
                return key;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex.StackTrace, ex.Source);
                return "Remove From Database";
            }
        }

        public static string PrintSong(SongTableObject tableObject)
        {
            return $"{tableObject.Show} {tableObject.Type} {tableObject.SongName} by {tableObject.Artist}";
        }

        public static string PrintSong(PlayerTableObject tableObject)
        {
            return $"{tableObject.Show} {tableObject.Type} {tableObject.SongName} by {tableObject.Artist}";
        }
    }

    //This class represents a player's stats on a given song when the "condition" or rule is met.
    // For example the rule Ranked means this object represents a given players' stats on the specific song in ranked
    public class PlayerTableObject
    {
        [Key]
        public string Key { get; set; }
        public string PlayerName { get; set; }
        public int TotalTimesPlayed { get; set; }
        public int TimesCorrect { get; set; }
        public int FromList { get; set; }
        public string Rule { get; set; }
        public int AnnID { get; set; }
        public string Type { get; set; }
        public string Artist { get; set; }
        public string SongName { get; set; }
        public int AnnSongID { get; set; }
        public string Show { get; set; }
        public string Romaji { get; set; }

        public PlayerTableObject()
        {
            Key = "";
            PlayerName = "";
            TotalTimesPlayed = 0;
            TimesCorrect = 0;
            FromList = 0;
        }
        public PlayerTableObject(int annId, string type, string songname, string artist, string player, int list, string rule)
        {
            PlayerName = player;
            TotalTimesPlayed = 0;
            TimesCorrect = 0;
            FromList = list;
            Rule = rule;
            Type = type;
            AnnID = annId;
            SongName = songname;
            Artist = artist;
            Key = MakePlayerTableKey(annId, type, SongName, Artist, PlayerName, Rule);
        }
        public PlayerTableObject(SongTableObject song, string player, int list, bool correct, string rule)
        {
            PlayerName = player;
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
            Show = song.Show;
            Artist = song.Artist;
            Type = song.Type;
            SongName = song.SongName;
            AnnID = song.AnnID;
            Romaji = song.Romaji;
            Key = MakePlayerTableKey(song.AnnID, song.Type, song.SongName, song.Artist, PlayerName, Rule);
        }
        public PlayerTableObject(PlayerTableObject tableObject, string newName)
        {
            PlayerName = newName;
            TotalTimesPlayed = tableObject.TotalTimesPlayed;
            TimesCorrect = tableObject.TimesCorrect;
            FromList = tableObject.FromList;
            Rule = tableObject.Rule;
            Show = tableObject.Show;
            AnnID = tableObject.AnnID;
            Artist = tableObject.Artist;
            Show = tableObject.Show;
            Type = tableObject.Type;
            SongName = tableObject.SongName;
            AnnSongID = tableObject.AnnSongID;
            Romaji = tableObject.Romaji;
            Key = MakePlayerTableKey(tableObject.AnnID, tableObject.Type,
             tableObject.SongName, tableObject.Artist, newName, Rule);
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

        public static string MakePlayerTableKey(int AnnID, string songtype, string songname, string artist, string playername, string rule)
        {
            return $"{AnnID} {songtype.ToLower()} {songname.ToLower()} by {artist.ToLower()} {playername.ToLower()} {rule.ToLower()}";
        }

        public static string PrintPlayer(PlayerTableObject player)
        {
            return $"{player.Show} {player.Type} {player.SongName} by {player.Artist} {player.PlayerName} {player.Rule}";
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

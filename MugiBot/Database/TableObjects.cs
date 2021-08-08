﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PartyBot.Database
{
    //This is an object which represents a unique song and holds links to catbox files for 
    //the LavalinkAudio class to play.
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
        public SongTableObject(string song, string art, string t, string Showname, string Roma, string u, string v)
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
        public SongTableObject(string song, string art, string t, string Showname, string Roma, string u, int Id, string _720link)
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
        public SongTableObject(string song, string art, string t, string Showname, string Roma, string u, int Id, string _720link, string _480link)
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
        }
        public static string MakeSongTableKey(int AnnID, string songtype, string songname, string artist)
        {
            try
            {
                string key = AnnID + " " + songtype.ToLower() + " " + songname.ToLower() + " by " + artist.ToLower();
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
                string key = pt.AnnID + " " + pt.Type.ToLower() + " " + pt.SongName.ToLower() + " by " + pt.Artist.ToLower();
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
        public SongTableObject SongObject { get; set; }

        public PlayerTableObject()
        {
            Key = "";
            PlayerName = "";
            TotalTimesPlayed = 0;
            TimesCorrect = 0;
            FromList = 0;
        }
        public PlayerTableObject(SongTableObject song, string player, int list, string rule)
        {
            PlayerName = player;
            TotalTimesPlayed = 0;
            TimesCorrect = 0;
            FromList = list;
            Rule = rule;
            Key = MakePlayerTableKey(song.AnnID, song.Type, song.SongName, song.Artist, PlayerName, Rule);
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
            Key = MakePlayerTableKey(song.AnnID, song.Type, song.SongName, song.Artist, PlayerName, Rule);
        }
        public PlayerTableObject(PlayerTableObject tableObject, string newName)
        {
            Show = tableObject.SongObject.Show;
            Romaji = tableObject.SongObject.Romaji;
            Type = tableObject.SongObject.Type;
            SongName = tableObject.SongObject.SongName;
            PlayerName = newName;
            TotalTimesPlayed = tableObject.TotalTimesPlayed;
            TimesCorrect = tableObject.TimesCorrect;
            FromList = tableObject.FromList;
            Rule = tableObject.Rule;
            Artist = tableObject.SongObject.Artist;
            Key = MakePlayerTableKey(song.AnnID, song.Type, song.SongName, song.Artist, newName, Rule);
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
            return AnnID + " " + songtype.ToLower() + " " +
                songname.ToLower() + " by " + artist.ToLower() + " " + playername.ToLower() + " " + rule.ToLower();
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

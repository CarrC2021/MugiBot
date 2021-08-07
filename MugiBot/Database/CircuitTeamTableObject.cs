using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PartyBot.Database
{
    public class CircuitTeamTableObject
    {
        [Key]
        public string Key { get; set; }
        public string TeamName { get; set; }
        public int TotalTimesPlayed { get; set; }
        public string Artist { get; set; }
        public int TimesCorrect { get; set; }
        public int FromList { get; set; }
        public string Type { get; set; }
        public string Show { get; set; }
        public string SongName { get; set; }
        public string Romaji { get; set; }
        public int CircuitNumber { get; set; }
        public int StageNumber { get; set; }
        public int AnnID { get; set; }

        public CircuitTeamTableObject()
        {
            Key = "";
            TeamName = "";
            TotalTimesPlayed = 0;
            Artist = "";
            TimesCorrect = 0;
            FromList = 0;
            Type = "";
            Show = "";
            SongName = "";
            Romaji = "";

        }

        public CircuitTeamTableObject(string showname, string roma, string songName, string t, string teamName, 
            string artist, int list, bool correct, int cNum, int sNum, int annId)
        {
            Show = showname;
            Romaji = roma;
            Type = t;
            TeamName = teamName;
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
            CircuitNumber = cNum;
            StageNumber = sNum;
            AnnID = annId;
            Key = MakeCircuitTableKey(Show, Type, SongName, artist, TeamName, CircuitNumber, StageNumber);
        }

        public static string MakeCircuitTableKey(string Show, string Type, string SongName, string artist, string TeamName, int CircuitNumber, int StageNumber)
        {
            return $"{Show} {SongName} {artist} {TeamName} {CircuitNumber} {StageNumber}";
        }
    }

    public class CircuitTeamTableObjectComparer : IEqualityComparer<CircuitTeamTableObject>
    {
        public bool Equals(CircuitTeamTableObject x, CircuitTeamTableObject y)
        {
            if (x.Key.Equals(y.Key))
            {
                return true;
            }
            return false;
        }
        public int GetHashCode(CircuitTeamTableObject obj)
        {
            return obj.Key.GetHashCode();
        }
    }
}
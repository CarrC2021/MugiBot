using System.Collections.Generic;

namespace PartyBot.DataStructs
{
    public class Playlist
    {
        public string Author { get; set; }
        public Dictionary<string, string> Songs { get; set; }

        public Playlist(string a, Dictionary<string, string> dict)
        {
            Author = a;
            Songs = dict;
        }
    }
}
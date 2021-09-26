using System.Collections.Generic;

namespace PartyBot.DataStructs
{
    public class Playlist
    {
        // Creator of the playlist.
        public string Author { get; set; }

        // This is a setting for private playlists. Toggles whether other people can view the contents of the playlist.
        public bool Viewable { get; set; }
        // A dictionary with songkeys for keys and the printed out version of songs for values.
        public Dictionary<string, string> Songs { get; set; }

        public Playlist(string a, Dictionary<string, string> dict)
        {
            Author = a;
            Songs = dict;
        }
    }
}
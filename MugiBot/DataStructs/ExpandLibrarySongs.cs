using Newtonsoft.Json;
using System.Collections.Generic;

namespace PartyBot.DataStructs
{

    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class Examples
    {
        [JsonProperty("720")]
        public string _720 { get; set; }

        [JsonProperty("mp3")]
        public string Mp3 { get; set; }

        [JsonProperty("480")]
        public string _480 { get; set; }
    }

    public class Catbox
    {
        [JsonProperty("480")]
        public int _480 { get; set; }

        [JsonProperty("720")]
        public int _720 { get; set; }

        [JsonProperty("mp3")]
        public int Mp3 { get; set; }
    }

    public class Open
    {
        [JsonProperty("catbox")]
        public Catbox Catbox { get; set; }
    }

    public class Openingsmoe
    {
        [JsonProperty("resolution")]
        public int? Resolution { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }
    }

    public class Closed
    {
        [JsonProperty("openingsmoe")]
        public Openingsmoe Openingsmoe { get; set; }
    }

    public class Versions
    {
        [JsonProperty("open")]
        public Open Open { get; set; }

        [JsonProperty("closed")]
        public Closed Closed { get; set; }
    }

    public class Song
    {
        [JsonProperty("annSongId")]
        public int AnnSongId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("number")]
        public int Number { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("examples")]
        public Examples Examples { get; set; }

        [JsonProperty("versions")]
        public Versions Versions { get; set; }
    }

    public class Question
    {
        // This class represents one anime and contains its id, name, and the songs from the show.
        [JsonProperty("annId")]
        public int AnnId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("songs")]
        public List<Song> Songs { get; } = new List<Song>();
    }
}
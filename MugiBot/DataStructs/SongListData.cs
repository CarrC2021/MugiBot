using Newtonsoft.Json;

/// <summary>
/// This file is meant to contain all the necessary classes to deserialize
/// Jsons exported from the AMQ SongList user script made by TheJoseph98 which
/// can be found here https://github.com/TheJoseph98/AMQ-Scripts
/// <summary>

namespace PartyBot.DataStructs
{

    public class SongListData
    {
        [JsonProperty("animeEng")]
        public readonly string animeEng;
        [JsonProperty("animeRomaji")]
        public readonly string animeRomaji;
        [JsonProperty("songName")]
        public readonly string songName;
        [JsonProperty("artist")]
        public readonly string artist;
        [JsonProperty("type")]
        public readonly string type;
        [JsonProperty("correctCount")]
        public readonly int correctCount;
        [JsonProperty("startTime")]
        public readonly int startTime;
        [JsonProperty("songDuration")]
        public readonly float songDuration;
        [JsonProperty("songNumber")]
        public readonly int songNumber;
        [JsonProperty("activePlayerCount")]
        public readonly int activePlayerCount;
        [JsonProperty("LinkVideo")]
        public readonly string LinkVideo;
        [JsonProperty("LinkMp3")]
        public readonly string LinkMp3;
        [JsonProperty("annId")]
        public readonly int annId;
        [JsonProperty("correctplayers")]
        public readonly string[] correctPlayers;
    }


}

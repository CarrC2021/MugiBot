﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

/// <summary>
/// This file is meant to contain all the necessary classes to deserialize
/// Jsons exported from the AMQ SongListUI user script made by TheJoseph98 which
/// can be found here https://github.com/TheJoseph98/AMQ-Scripts
/// <summary>
namespace PartyBot.DataStructs
{
    public class GameData
    {

        //A list which represents each song that played,
        //each individual object holds all information recorded
        //in the lobby.
        public List<SongData> songs { get; set; }
        public GameData(List<SongData> s)
        {
            songs = s;
        }
    }

    public class SongData
    {
        //The current game mode
        [JsonProperty("gameMode")]
        public string gameMode { get; set; }
        //name of the song
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("artist")]
        public string artist { get; set; }
        //The anime the song is from
        [JsonProperty("anime")]
        public Anime anime { get; set; }
        //Anime News Network ID of the show from which the song played
        [JsonProperty("annId")]
        public int annId { get; set; }
        [JsonProperty("siteIds")]
        public SiteIDs SiteIDs { get; set; }
        //the number that comes after type, ex OP 2 or ED 3
        [JsonProperty("songNumber")]
        public int songNumber { get; set; }
        [JsonProperty("activePlayers")]
        public int activePlayers { get; set; }
        [JsonProperty("totalPlayers")]
        public int totalPlayers { get; set; }
        //OP, Ed, or insert
        [JsonProperty("type")]
        public string type { get; set; }
        [JsonProperty("urls")]
        public Urls urls { get; set; }
        [JsonProperty("startSample")]
        public int startSample { get; set; }
        //length of the video
        [JsonProperty("videoLength")]
        public float videoLength { get; set; }
        //all players in the lobby or other players?
        [JsonProperty("players")]
        public List<Player> players { get; set; }
        [JsonProperty("fromList")]
        public List<Fromlist> fromList { get; set; }
        [JsonProperty("correct")]
        public bool correct { get; set; }
        //answer typed by the player
        [JsonProperty("selfAnswer")]
        public string selfAnswer { get; set; }

        public string MakeSongTableKey()
        {
            try
            {
                string key = $"{annId} {type.ToLower()} {name.ToLower()} by {artist.ToLower()}";
                return key;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex.StackTrace, ex.Source);
                return "Remove From Database";
            }
        }
    }

    public class Anime
    {
        //English translation of the title
        [JsonProperty("english")]
        public string english { get; set; }
        //Romaji representation of the title
        [JsonProperty("romaji")]
        public string romaji { get; set; }
    }

    public class SiteIDs
    {
        [JsonProperty("annId")]
        public int annId { get; set; }
        [JsonProperty("malId")]
        public int malId { get; set; }
        [JsonProperty("kitsuId")]
        public int kitsuId { get; set; }
        [JsonProperty("aniListId")]
        public int aniListId { get; set; }
    }
    public class Urls
    {
        [JsonProperty("catbox")]
        public CatBox catbox { get; set; }
        [JsonProperty("openingsmoe")]
        public OpeningsMoe openingsmoe { get; set; }
    }

    public class CatBox
    {
        //link to MP3 file hosted on Catbox
        [JsonProperty("0")]
        public string _0 { get; set; }
        [JsonProperty("480")]
        public string _480 { get; set; }
        //link to HD video file hosted on Catbox
        [JsonProperty("720")]
        public string _720 { get; set; }
    }

    public class OpeningsMoe
    {
        //link to HD video hosted on OpeningsMoe
        [JsonProperty("720")]
        public string _720 { get; set; }
    }

    public class Player
    {
        //Name of the player
        [JsonProperty("name")]
        public string name { get; set; }
        //score the player gave the show on MAL/Anilist etc
        [JsonProperty("score")]
        public int score { get; set; }
        //did the Player get the correct answer
        [JsonProperty("correct")]
        public bool correct { get; set; }
        //answer put down
        [JsonProperty("answer")]
        public string answer { get; set; }
        //time it took to guess
        [JsonProperty("guessTime")]
        public System.Int64 guessTime { get; set; }
        //was the player active
        [JsonProperty("active")]
        public bool active { get; set; }
        //what position in the lobby was the player
        [JsonProperty("position")]
        public int position { get; set; }
        [JsonProperty("positionSlot")]
        public int positionSlot { get; set; }
    }

    public class Fromlist
    {
        //name of the player
        [JsonProperty("name")]
        public string name { get; set; }
        //in their list or not
        [JsonProperty("listStatus")]
        public int listStatus { get; set; }
        [JsonProperty("score")]
        public object score { get; set; }

        public Fromlist(string n, int list, object s)
        {
            name = n;
            listStatus = list;
            score = s;
        }
        public Fromlist()
        {
            name = null;
            listStatus = 400;
            score = null;
        }
    }
}


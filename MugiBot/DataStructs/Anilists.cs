using System.Collections.Generic;
using Newtonsoft.Json;

namespace PartyBot.DataStructs
{ 
    public class Title
    {
        [JsonProperty("romaji")]
        public string Romaji { get; set; }

        [JsonProperty("english")]
        public string English { get; set; }
    }

    public class CoverImage
    {
        [JsonProperty("extraLarge")]
        public string ExtraLarge { get; set; }

        [JsonProperty("large")]
        public string Large { get; set; }

        [JsonProperty("medium")]
        public string Medium { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }
    }

    public class AnimeMedia
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("idMal")]
        public int IdMal { get; set; }

        [JsonProperty("season")]
        public string Season { get; set; }

        [JsonProperty("seasonYear")]
        public int? SeasonYear { get; set; }

        [JsonProperty("title")]
        public Title Title { get; set; }

        [JsonProperty("coverImage")]
        public CoverImage CoverImage { get; set; }

        [JsonProperty("bannerImage")]
        public string BannerImage { get; set; }

        [JsonProperty("genres")]
        public List<string> Genres { get; } = new List<string>();

        [JsonProperty("mediaListEntry")]
        public object MediaListEntry { get; set; }

        [JsonProperty("siteUrl")]
        public string SiteUrl { get; set; }
    }

    public class Entry
    {
        [JsonProperty("media")]
        public AnimeMedia Media { get; set; }
    }

    public class List
    {
        [JsonProperty("entries")]
        public List<Entry> Entries { get; } = new List<Entry>();
    }

    public class MediaListCollection
    {
        [JsonProperty("lists")]
        public List<List> Lists { get; } = new List<List>();
    }

    public class UserAnilist
    {
        [JsonProperty("MediaListCollection")]
        public MediaListCollection MediaListCollection { get; set; }
    }

    public class AnilistData
    {
        [JsonProperty("data")]
        public UserAnilist UserAnilist { get; set; }
    }

    public class UserListResponse
    {
        public AnilistData AnilistData { get; set; }
    }

}
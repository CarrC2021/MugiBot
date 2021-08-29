// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PartyBot.DataStructs
{
    public class AdvancedScores
    {
        [JsonPropertyName("Story")]
        public int Story { get; set; }

        [JsonPropertyName("Characters")]
        public int Characters { get; set; }

        [JsonPropertyName("Visuals")]
        public int Visuals { get; set; }

        [JsonPropertyName("Audio")]
        public int Audio { get; set; }

        [JsonPropertyName("Enjoyment")]
        public int Enjoyment { get; set; }
    }

    public class StartedAt
    {
        [JsonPropertyName("year")]
        public int? Year { get; set; }

        [JsonPropertyName("month")]
        public int? Month { get; set; }

        [JsonPropertyName("day")]
        public int? Day { get; set; }
    }

    public class CompletedAt
    {
        [JsonPropertyName("year")]
        public int? Year { get; set; }

        [JsonPropertyName("month")]
        public int? Month { get; set; }

        [JsonPropertyName("day")]
        public int? Day { get; set; }
    }

    public class Title
    {
        [JsonPropertyName("romaji")]
        public string Romaji { get; set; }

        [JsonPropertyName("native")]
        public string Native { get; set; }

        [JsonPropertyName("english")]
        public string English { get; set; }
    }

    public class Media
    {
        [JsonPropertyName("idMal")]
        public int IdMal { get; set; }

        [JsonPropertyName("title")]
        public Title Title { get; set; }
    }

    public class Entry
    {
        [JsonPropertyName("mediaId")]
        public int MediaId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("progress")]
        public int Progress { get; set; }

        [JsonPropertyName("repeat")]
        public int Repeat { get; set; }

        [JsonPropertyName("notes")]
        public string Notes { get; set; }

        [JsonPropertyName("priority")]
        public int Priority { get; set; }

        [JsonPropertyName("hiddenFromStatusLists")]
        public bool HiddenFromStatusLists { get; set; }

        [JsonPropertyName("customLists")]
        public object CustomLists { get; set; }

        [JsonPropertyName("advancedScores")]
        public AdvancedScores AdvancedScores { get; set; }

        [JsonPropertyName("startedAt")]
        public StartedAt StartedAt { get; set; }

        [JsonPropertyName("completedAt")]
        public CompletedAt CompletedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public int UpdatedAt { get; set; }

        [JsonPropertyName("createdAt")]
        public int CreatedAt { get; set; }

        [JsonPropertyName("media")]
        public Media Media { get; set; }

        [JsonPropertyName("score")]
        public double Score { get; set; }
    }

    public class List
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("isCustomList")]
        public bool IsCustomList { get; set; }

        [JsonPropertyName("isSplitCompletedList")]
        public bool IsSplitCompletedList { get; set; }

        [JsonPropertyName("entries")]
        public List<Entry> Entries { get; } = new List<Entry>();
    }

    public class MediaListCollection
    {
        [JsonPropertyName("lists")]
        public List<List> Lists { get; } = new List<List>();
    }

    public class MediaListOptions
    {
        [JsonPropertyName("scoreFormat")]
        public string ScoreFormat { get; set; }
    }

    public class User
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("mediaListOptions")]
        public MediaListOptions MediaListOptions { get; set; }
    }

    public class ScriptInfo
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }

        [JsonPropertyName("repo")]
        public string Repo { get; set; }

        [JsonPropertyName("firefox")]
        public string Firefox { get; set; }

        [JsonPropertyName("chrome")]
        public string Chrome { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("authorLink")]
        public string AuthorLink { get; set; }

        [JsonPropertyName("license")]
        public string License { get; set; }
    }

    public class Root
    {
        [JsonPropertyName("MediaListCollection")]
        public MediaListCollection MediaListCollection { get; set; }

        [JsonPropertyName("User")]
        public User User { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("scriptInfo")]
        public ScriptInfo ScriptInfo { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("timeStamp")]
        public long TimeStamp { get; set; }
    }


}

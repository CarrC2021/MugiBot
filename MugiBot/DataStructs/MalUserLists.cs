// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PartyBot.DataStructs
{ 
    public class Datum
    {
        [JsonProperty("node")]
        public Node Node { get; set; }

        [JsonProperty("list_status")]
        public ListStatus ListStatus { get; set; }
    }

    public class ListStatus
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("num_episodes_watched")]
        public int NumEpisodesWatched { get; set; }

        [JsonProperty("is_rewatching")]
        public bool IsRewatching { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("start_date")]
        public string StartDate { get; set; }
    }

    public class MainPicture
    {
        [JsonProperty("medium")]
        public string Medium { get; set; }

        [JsonProperty("large")]
        public string Large { get; set; }
    }

    public class Node
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("main_picture")]
        public MainPicture MainPicture { get; set; }
    }

    public class Paging
    {
        [JsonProperty("next")]
        public string Next { get; set; }

        public Paging(string next)
        {
            Next = next;
        }
    }

    public class MalUserList
    {
        [JsonProperty("data")]
        public List<Datum> Data { get; } = new List<Datum>();

        [JsonProperty("paging")]
        public Paging Paging { get; set; }

        public MalUserList(List<Datum> data, Paging paging)
        {
            Data = data;
            Paging = paging;
        }
    }
}

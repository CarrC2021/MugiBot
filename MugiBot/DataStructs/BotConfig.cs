using System.Collections.Generic;
using Newtonsoft.Json;

namespace PartyBot.DataStructs
{
    public class BotConfig
    {
        public string DiscordToken { get; set; }
        public string DefaultPrefix { get; set; }
        public string GameStatus { get; set; }
        public List<ulong> BlacklistedChannels { get; set; }
        public string LocalEndPoint { get; set; }
        
        [JsonProperty("DatabaseAdmins")]
        public List<ulong> DatabaseAdmins { get; set; }
    }
}

using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using PartyBot.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using PartyBot.DataStructs;
using System.Text;
using Discord.WebSocket;
using PartyBot.Database;

namespace PartyBot.Services
{
    public sealed class PlayersRulesService
    {
        private readonly char separator = Path.DirectorySeparatorChar;
        public string mainpath { get; set; }
        private string rulesPath;
        private string playersPath;
        private string usernamesPath;
        private JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public PlayersRulesService() {}

        public void SetSubPaths()
        {
            rulesPath = Path.Combine(mainpath, "Database", "rules.txt");
            playersPath = Path.Combine(mainpath, "Database", "players.json");
            usernamesPath = Path.Combine(mainpath, "Database", "usernames.json");
        }
        public async Task<List<string>> RulesMetBySongData(SongData song, Dictionary<string, string> dict)
        {
            List<string> RulesMet = new List<string>();
            //If any of the rules is equivalent to the gamemode, i.e solo or ranked then add it
            if (song.gameMode == "Solo" || song.gameMode == "Ranked")
                RulesMet.Add(song.gameMode);
            return await Task.Run(() => AllPlayersInLobby(RulesMet, song, dict));
        }

        /// <summary>
        /// This function will iterate through every Player in the SongData object passed to it.
        /// If one of the rules that specifies which players need to be in the lobby is met then,
        /// that rule will be added to the RulesMet parameter and will be returned in its updated state.
        /// <summary>
        /// <returns> a List of strings once the asynchronous task is completed. </returns>
        private async Task<List<string>> AllPlayersInLobby(List<string> RulesMet, SongData song, Dictionary<string, string> dict)
        {
            List<string> rules = await GetRules();
            foreach (string rule in rules)
            {
                List<string> list = rule.Split(" ").ToList();
                int correct = 0;
                foreach (Player player in song.players)
                {
                    var name = dict.TryGetValue(player.name, out string value);
                    //Check if the player's name is part of the rule 
                    if (list.Exists(p => p.Equals(value)))
                        correct += 1;
                }
                //If all of the players to keep track of were active in the lobby then the rule has been met for this song
                if (correct >= list.Count())
                    RulesMet.Add(rule);
            }
            return RulesMet;
        }
        // Tracks the given AMQ user name in the database under the second name provided
        public async Task<Embed> TrackPlayer(string AMQUserName, string nameInDB)
        {
            string contents = await File.ReadAllTextAsync(playersPath);
            Dictionary<string, string> tempDict = await Task.Run(() =>
                JsonConvert.DeserializeObject<Dictionary<string, string>>(contents, settings));
            tempDict.TryAdd(AMQUserName, nameInDB);
            await File.WriteAllTextAsync(playersPath, JsonConvert.SerializeObject(tempDict));
            return await EmbedHandler.CreateBasicEmbed("Data", $"{AMQUserName} will now have their stats tracked in the database as {nameInDB}.", Color.Blue);
        }

        // Removes the player from the players.json file so that they will no longer be tracked.
        public async Task<Embed> RemovePlayer(string name)
        {
            string contents = await File.ReadAllTextAsync(playersPath);
            Console.WriteLine(contents);
            Dictionary<string, string> tempDict = await Task.Run(() =>
                JsonConvert.DeserializeObject<Dictionary<string, string>>(contents, settings));
            try
            {
                tempDict.Remove(name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            await File.WriteAllTextAsync(playersPath, JsonConvert.SerializeObject(tempDict));
            return await EmbedHandler.CreateBasicEmbed("Data", $"{name} will no longer have their stats tracked.", Color.Blue);
        }

        // Returns the players.json file deserialized to a dictionary
        // with string keys and string values.
        public async Task<Dictionary<string, string>> GetPlayersTracked()
        {
            string contents = await File.ReadAllTextAsync(playersPath);
            Dictionary<string, string> tempDict = await Task.Run(() =>
                JsonConvert.DeserializeObject<Dictionary<string, string>>(contents, settings));
            return tempDict;
        }

        // Returns an embed to print to discord which has all the players tracked in the database.
        public async Task<Embed> ListPlayersTracked(ISocketMessageChannel channel)
        {
            var playersDict = await GetPlayersTracked();
            var sb = new StringBuilder();
            sb.Append("the --> means, tracked in the database as \n");
            foreach (string playerName in playersDict.Keys)
            {
                if ((sb.ToString() + $"{playerName} --> {playersDict[playerName]}\n").Length >= 2048)
                {
                    await channel.SendMessageAsync(embed: await EmbedHandler.CreateBasicEmbed("Data", sb.ToString(), Color.Blue));
                    sb.Clear();
                }
                sb.Append($"{playerName} --> {playersDict[playerName]}\n");
            }
            return await EmbedHandler.CreateBasicEmbed("Data", sb.ToString(), Color.Blue);
        }

        // Returns the rules the bot currently keeps track of.
        public async Task<List<string>> GetRules()
        {
            string[] array = await File.ReadAllLinesAsync(rulesPath);
            return array.ToList();
        }

        public async Task<Embed> NewRule([Remainder] string rule)
        {
            await File.AppendAllTextAsync(rulesPath, "\n" + rule);
            return await EmbedHandler.CreateBasicEmbed("Data", $"{rule} has been added to the rules file", Color.Blue);
        }

        public async Task<Embed> ListRules()
        {
            string toPrint = "";
            foreach (string rule in await GetRules())
                toPrint += $"{rule}\n";

            return await EmbedHandler.CreateBasicEmbed("Data", toPrint, Color.Blue);
        }

        public async Task<Embed> DeleteRule(string rule)
        {
            await Task.Run(() => File.WriteAllLines(rulesPath,
               File.ReadLines(rulesPath).Where(l => l != rule).ToList()));
            return await EmbedHandler.CreateBasicEmbed("Data", rule + " has been deleted.", Color.Blue);
        }
    }
}


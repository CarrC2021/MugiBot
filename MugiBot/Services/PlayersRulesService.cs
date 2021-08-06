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

namespace PartyBot.Services
{
    public sealed class PlayersRulesService
    {
        private readonly char separator = Path.DirectorySeparatorChar;
        private readonly string mainpath;
        private readonly string rulesPath;
        private readonly string playersPath;
        private readonly string usernamesPath;
        private JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public PlayersRulesService()
        {
            //Making sure that all of the pathing will work regardless of platform
            mainpath = Path.GetDirectoryName(System.Reflection.
            Assembly.GetExecutingAssembly().GetName().CodeBase).Replace($"{separator}" +
            $"bin{separator}Debug{separator}netcoreapp3.1", "").Replace($"file:{separator}", "");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                mainpath = separator + mainpath;
            rulesPath = Path.Combine(mainpath, "Database", "rules.txt");
            playersPath = Path.Combine(mainpath, "Database", "players.json");
            usernamesPath = Path.Combine(mainpath, "Database", "usernames.json");
        }

        public async Task<List<string>> RulesMetBySongData(SongData song, List<string> rules, Dictionary<string, string> dict)
        {
            List<string> RulesMet = new List<string>();
            //If any of the rules is equivalent to the gamemode, i.e solo or ranked then add it
            if (song.gameMode == "Solo" || song.gameMode == "Ranked")
                RulesMet.Add(song.gameMode);
            return await Task.Run(() => AllPlayersInLobby(rules, RulesMet, song, dict));
        }

        private List<string> AllPlayersInLobby(List<string> rules, List<string> RulesMet, SongData song, Dictionary<string, string> dict)
        {
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
                //If all of the players were in the lobby then the rule has been met
                if (correct >= list.Count())
                    RulesMet.Add(rule);
            }
            return RulesMet;
        }
        public async Task<Embed> TrackPlayer(string key, string value)
        {
            string contents = await File.ReadAllTextAsync(playersPath);
            Dictionary<string, string> tempDict = await Task.Run(() =>
                JsonConvert.DeserializeObject<Dictionary<string, string>>(contents, settings));
            tempDict.TryAdd(key, value);
            await File.WriteAllTextAsync(playersPath, JsonConvert.SerializeObject(tempDict));
            return await EmbedHandler.CreateBasicEmbed("Data", $"{key} will now have their stats tracked in the database as {value}.", Color.Blue);
        }

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

        public async Task<Dictionary<string, string>> GetPlayersTracked()
        {
            string contents = await File.ReadAllTextAsync(playersPath);
            Dictionary<string, string> tempDict = await Task.Run(() =>
                JsonConvert.DeserializeObject<Dictionary<string, string>>(contents, settings));
            return tempDict;
        }

        public async Task<Dictionary<string, string>> GetPlayersTrackedLower()
        {
            var players = await GetPlayersTracked();
            Dictionary<string, string> lowerCase = new Dictionary<string, string>();
            foreach (var key in players.Keys)
                lowerCase.Add(key.ToLower(), players[key].ToLower());

            return lowerCase;
        }

        public async Task<Embed> ListPlayersTracked()
        {
            var playersDict = await GetPlayersTracked();
            string f = "the --> means, tracked in the database as \n";
            foreach (string s in playersDict.Keys)
                f += $"{s} --> {playersDict[s]}\n";
            return await EmbedHandler.CreateBasicEmbed("Data", f, Color.Blue);
        }

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

        public async Task<Embed> SetUsername(IUserMessage message, string username)
        {
            string contents = await File.ReadAllTextAsync(usernamesPath);
            Dictionary<ulong, string> tempDict = await Task.Run(() =>
                JsonConvert.DeserializeObject<Dictionary<ulong, string>>(contents, settings));
            tempDict.TryAdd(((ulong)message.Author.Id), username);
            await File.WriteAllTextAsync(usernamesPath, JsonConvert.SerializeObject(tempDict));
            return await EmbedHandler.CreateBasicEmbed("Data", $"Your AMQ username is now set to {username}.", Color.Blue);
        }

        public async Task<Dictionary<ulong, string>> GetUsernameValues()
        {
            string contents = await File.ReadAllTextAsync(usernamesPath);
            Dictionary<ulong, string> tempDict = await Task.Run(() =>
                JsonConvert.DeserializeObject<Dictionary<ulong, string>>(contents, settings));
            return tempDict;
        }

        public async Task<Embed> ListUsernameAssignments(IGuild guild)
        {
            var usernamesDict = await GetUsernameValues();
            string f = "";
            foreach (ulong ID in usernamesDict.Keys)
            {
                var user = await guild.GetUserAsync(ID, CacheMode.AllowDownload);
                f += $"{user.Username} is on team {usernamesDict[user.Id]}\n";
            }
            return await EmbedHandler.CreateBasicEmbed("Data", f, Color.Blue);
        }

        public async Task<Embed> RemoveUsername(IUserMessage message)
        {
            string contents = await File.ReadAllTextAsync(usernamesPath);
            Console.WriteLine(contents);
            Dictionary<ulong, string> tempDict = await Task.Run(() =>
                JsonConvert.DeserializeObject<Dictionary<ulong, string>>(contents, settings));
            try
            {
                tempDict.Remove(((ulong)message.Author.Id));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            await File.WriteAllTextAsync(usernamesPath, JsonConvert.SerializeObject(tempDict));
            return await EmbedHandler.CreateBasicEmbed("Data", $"You have had your AMQ username removed, you can set it to a new value now.", Color.Blue);
        }
    }
}


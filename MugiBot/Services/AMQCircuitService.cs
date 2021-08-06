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
    public sealed class AMQCircuitService
    {
        private readonly char separator = Path.DirectorySeparatorChar;
        private readonly string mainpath;
        private readonly string teamsPath;
        private JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public AMQCircuitService()
        {
            //Making sure that all of the pathing will work regardless of platform
            mainpath = Path.GetDirectoryName(System.Reflection.
            Assembly.GetExecutingAssembly().GetName().CodeBase).Replace($"{separator}" +
            $"bin{separator}Debug{separator}netcoreapp3.1", "").Replace($"file:{separator}", "");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                mainpath = separator + mainpath;
            teamsPath = Path.Combine(mainpath, "Database", "teams.json");
        }
        public async Task<Embed> SetTeam(IUserMessage message, string team)
        {
            string contents = await File.ReadAllTextAsync(teamsPath);
            Dictionary<ulong, string> tempDict = await Task.Run(() =>
                JsonConvert.DeserializeObject<Dictionary<ulong, string>>(contents, settings));
            tempDict.TryAdd(((ulong)message.Author.Id), team);
            await File.WriteAllTextAsync(teamsPath, JsonConvert.SerializeObject(tempDict));
            return await EmbedHandler.CreateBasicEmbed("Data", $"Your team is now set to {team}.", Color.Blue);
        }

        public async Task<Embed> RemoveTeam(IUserMessage message)
        {
            string contents = await File.ReadAllTextAsync(teamsPath);
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
            await File.WriteAllTextAsync(teamsPath, JsonConvert.SerializeObject(tempDict));
            return await EmbedHandler.CreateBasicEmbed("Data", $"You have had your team removed, you can set it to a new value now.", Color.Blue);
        }

        public async Task<Dictionary<ulong, string>> GetPlayersTracked()
        {
            string contents = await File.ReadAllTextAsync(teamsPath);
            Dictionary<ulong, string> tempDict = await Task.Run(() =>
                JsonConvert.DeserializeObject<Dictionary<ulong, string>>(contents, settings));
            return tempDict;
        }

        public async Task<Embed> ListTeamAssignments(IGuild guild)
        {
            var playersDict = await GetPlayersTracked();
            string f = "";
            foreach (ulong ID in playersDict.Keys)
            {
                var user = await guild.GetUserAsync(ID, CacheMode.AllowDownload);
                f += $"{user.Username} is on team {playersDict[user.Id]}\n";
            }
            return await EmbedHandler.CreateBasicEmbed("Data", f, Color.Blue);
        }

    }
}


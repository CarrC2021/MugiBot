using Discord;
using Discord.WebSocket;
using PartyBot.DataStructs;
using PartyBot.Handlers;
using PartyBot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PartyBot.Database
{
    public class CircuitDBManager
    {
        private readonly char separator = Path.DirectorySeparatorChar;
        private readonly string mainpath;
        private readonly AMQDBContext _db;
        int CircuitNumber;
        int StageNumber;

        public CircuitDBManager(AMQDBContext context)
        {
            mainpath = Path.GetDirectoryName(System.Reflection.
            Assembly.GetExecutingAssembly().GetName().CodeBase).Replace($"{separator}bin{separator}Debug{separator}netcoreapp3.1", "").Replace($"file:{separator}", "");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                mainpath = separator + mainpath;
            IEnumerable<string> lines = File.ReadLines(Path.Combine(mainpath, "CircuitInfo.txt")); 
            CircuitNumber = Convert.ToInt32(lines.First());
            lines.Reverse();
            StageNumber = Convert.ToInt32(lines.First());
            _db = context;
        }

        public async Task<Embed> UpdateCircuitNumber(int cNum)
        {
            CircuitNumber = cNum;
            string[] lines = await File.ReadAllLinesAsync(Path.Combine(mainpath, "CircuitInfo.txt"));
            lines[0] = $"{cNum}";
            await File.WriteAllLinesAsync(Path.Combine(mainpath, "CircuitInfo.txt"), lines);
            return await EmbedHandler.CreateBasicEmbed("Circuit Info", $"Circuit Number now set to {cNum}", Color.Blue);
        }

        public async Task<Embed> UpdateStageNumber(int sNum)
        {
            StageNumber = sNum;
            string[] lines = await File.ReadAllLinesAsync(Path.Combine(mainpath, "CircuitInfo.txt"));
            lines[1] = $"{sNum}";
            await File.WriteAllLinesAsync(Path.Combine(mainpath, "CircuitInfo.txt"), lines);
            return await EmbedHandler.CreateBasicEmbed("Circuit Info", $"Stage Number now set to {sNum}", Color.Blue);
        }

        public async Task<Embed> CircuitInfo()
        {
            string[] lines = await File.ReadAllLinesAsync(Path.Combine(mainpath, "CircuitInfo.txt"));
            string toPrint = "";
            foreach (string line in lines)
                toPrint += $"{line}\n";
            return await EmbedHandler.CreateBasicEmbed("Circuit Info", toPrint, Color.Blue);
        }

        public async Task TeamStageInfo(SocketMessage message, string team = "Default")
        {
            if (!team.Equals("Default"))
            {
                var tableObjects = await GetCircuitTeamTablesAsync(team, CircuitNumber, StageNumber);
            }
                
        }
        public async Task<Embed> TeamCircuitInfo(IUserMessage message, string team = "Default")
        {
            if (!team.Equals("Default"))
            {
                var tableObjects = await GetCircuitTeamTablesAsync(team, CircuitNumber);
                return await PrintTeamObjects(tableObjects, true);
            }
            return await EmbedHandler.CreateBasicEmbed("Error", "Not implemented yet", Color.Red);
        }

        public async Task<List<CircuitTeamTableObject>> GetCircuitTeamTablesAsync(string teamName)
        {
            return await _db.CircuitTeams
                    .AsNoTracking()
                    .Where(f => f.TeamName.ToLower().Equals(teamName.ToLower()))
                    .ToListAsync();
        }

        public async Task<List<CircuitTeamTableObject>> GetCircuitTeamTablesAsync(string teamName, int c)
        {
            return await _db.CircuitTeams
                    .AsNoTracking()
                    .Where(f => f.TeamName.ToLower().Equals(teamName.ToLower()))
                    .Where(l => l.CircuitNumber == c)
                    .ToListAsync();
        }

        public async Task<List<CircuitTeamTableObject>> GetCircuitTeamTablesAsync(string teamName, int c, int s)
        {
            return await _db.CircuitTeams
                    .AsNoTracking()
                    .Where(f => f.TeamName.ToLower().Equals(teamName.ToLower()))
                    .Where(l => l.CircuitNumber == c)
                    .Where(y => y.CircuitNumber == s)
                    .ToListAsync();
        }

        public async Task<Embed> PrintTeamObjects(List<CircuitTeamTableObject> circuitTeamObjects, bool Circuit = false, bool Stage = false)
        {
            string name = circuitTeamObjects.FirstOrDefault().TeamName;
            string toPrint = $"{name}";
            if (Circuit)
                toPrint += $" {CircuitNumber}";
            if (Stage)
                toPrint += $" {StageNumber}";
            toPrint += "\n";
            foreach (CircuitTeamTableObject tObject in circuitTeamObjects)
            {
                toPrint += $"{tObject.Show} {tObject.Type} {tObject.SongName} by {tObject.Artist}\n";
                toPrint += $"\t Times played: {tObject.TotalTimesPlayed} and Times correct: {tObject.TimesCorrect}\n"; 
            }

            return await EmbedHandler.CreateBasicEmbed("Circuit", toPrint, Color.DarkGreen);
        }
    }
}
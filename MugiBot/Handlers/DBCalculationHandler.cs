using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using PartyBot.Database;
using PartyBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyBot.Handlers
{
    public static class DBCalculationHandler
    {

        public static async Task<Embed> ShowStats(ISocketMessageChannel ch, string show, bool exact = false)
        {
            return await CalcShowStats(ch, show, exact);
        }
        public static async Task<Embed> CalcTotalCorrect(PlayersRulesService _service, string rule = "")
        {
            using (var db = new AMQDBContext())
            {
                string successRates = "";
                var tempDict = await _service.GetPlayersTracked();
                var list = tempDict.Values.Distinct().ToList();
                foreach (string player in list)
                {
                    float total = 0;
                    float correct = 0;
                    var Shows = await db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.PlayerName.Equals(player))
                        .Where(j => j.Rule.Equals(rule))
                        .ToListAsync();
                    foreach (PlayerTableObject player1 in Shows)
                    {
                        total = total + player1.TotalTimesPlayed;
                        correct = correct + player1.TimesCorrect;
                    }
                    if (total > 0)
                    {
                        successRates = successRates + player + " correct rate: " + Math.Round(correct / total, 3) + "\n";
                    }
                }
                if (!rule.Equals(""))
                {
                    return await EmbedHandler.CreateBasicEmbed("Data", successRates, Color.Blue);
                }
                return await EmbedHandler.CreateBasicEmbed($"Rule: {rule}", successRates, Color.Blue);
            }
        }

        public static async Task<Embed> CalcShowStats(ISocketMessageChannel ch, string show, bool exact)
        {
            float total = 0;
            float correct = 0;
            List<PlayerTableObject> Query = new List<PlayerTableObject>();
            using (var db = new AMQDBContext())
            {
                if (exact)
                {
                    Query = await db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.Show.ToLower().Equals(show.ToLower()))
                        .Where(f => f.Rule.Equals(""))
                        .ToListAsync();
                }
                else
                {
                    Query = await db.PlayerStats
                        .AsNoTracking()
                        .Where(k => k.Show.ToLower().Contains(show.ToLower()))
                        .Where(f => f.Rule.Equals(""))
                        .ToListAsync();
                }
            }
            var dict = new Dictionary<string, float[]>();
            foreach (PlayerTableObject t in Query)
            {
                total += t.TotalTimesPlayed;
                correct += t.TimesCorrect;
                string key = SongTableObject.MakeSongTableKey(t.Show, t.Type, t.SongName, t.Artist);
                if (dict.ContainsKey(key))
                {
                    dict.TryGetValue(key, out var currentCount);
                    dict[key] = new float[] { currentCount[0] + t.TotalTimesPlayed, currentCount[1] + t.TimesCorrect };
                }
                else
                {
                    dict.Add(key, new float[] { t.TotalTimesPlayed, t.TimesCorrect });
                }
            }
            string list = $"Total Times Played: {total}, Total Times Correct: {correct}, Success Rate {correct / total}";
            int count = 0;
            foreach (var entry in dict)
            {
                //list = list + player.Key + " played: " +
                //player.TotalTimesPlayed + " correct: " + player.TimesCorrect + "\n";
                list += "\n" + entry.Key + "\n" + $"\t Times Played: {entry.Value[0]} Times Correct: {entry.Value[1]}";
                count++;
                if (count % 10 == 0)
                {
                    await ch.SendMessageAsync(embed: await EmbedHandler.CreateBasicEmbed($"Stats for {show}", list, Color.Blue));
                    list = "";
                }
            }
            return await EmbedHandler.CreateBasicEmbed($"Stats for {show}", list, Color.Blue);
        }
        public static async Task<Embed> RecommendPracticeSongs(ISocketMessageChannel ch, string name, int numSongs, bool onlyFromList)
        {
            string temp = "";
            Dictionary<string, int[]> playerSpecific = new Dictionary<string, int[]>();
            Dictionary<string, int[]> total = new Dictionary<string, int[]>();
            List<PlayerTableObject> OtherQuery = new List<PlayerTableObject>();
            using (var db = new AMQDBContext())
            {
                //We will only look at songs that the player has seen in game at least once.
                var PlayerQuery = await SearchHandler.AllObjectsForPlayer(db, name, onlyFromList);



                foreach (PlayerTableObject tObject in PlayerQuery)
                {
                    temp = SongTableObject.MakeSongTableKey(tObject.Show, tObject.Type, tObject.SongName, tObject.Artist);
                    total.Add(temp, new int[] { tObject.TotalTimesPlayed, tObject.TimesCorrect });
                    playerSpecific.Add(temp, new int[] { tObject.TotalTimesPlayed, tObject.TimesCorrect });
                }

                OtherQuery = await db.PlayerStats
                            .AsAsyncEnumerable()
                            .Where(x => x.Rule.Equals(""))
                            .Where(j => !j.PlayerName.ToLower().Equals(name.ToLower()))
                            .Where(y => total.ContainsKey(SongTableObject.MakeSongTableKey(y.Show, y.Type, y.SongName, y.Artist)))
                            .ToListAsync();
            }

            Dictionary<string, float[]> songsToRecommend = new Dictionary<string, float[]>();
            int num = 0;
            while (songsToRecommend.Count < numSongs)
            {
                songsToRecommend.Add($"placeholder key {num}", new float[] {1.0f,
                    1.0f});
                num++;
            }

            //now we will go through all of those objects and increment the stats
            foreach (PlayerTableObject tableObject in OtherQuery)
            {
                temp = SongTableObject.MakeSongTableKey(tableObject.Show, tableObject.Type, tableObject.SongName, tableObject.Artist);
                total.TryGetValue(temp, out var currentCount);
                total[temp] = new int[] { currentCount[0] + tableObject.TotalTimesPlayed, currentCount[1] + tableObject.TimesCorrect };
            }

            //now we will go through all of the song keys and calculate the difference between
            //two rates
            List<string> allKeys = total.Keys.ToList();
            foreach (string key in allKeys)
            {
                float allTimesPlayed = total[key][0];
                float allTimesCorrect = total[key][1];
                float totalSuccessRate = allTimesCorrect / allTimesPlayed;
                float timesPlayedForPlayer = playerSpecific[key][0];
                float timesCorrectForPlayer = playerSpecific[key][1];
                float playerSuccessRate = timesCorrectForPlayer / timesPlayedForPlayer;
                float diff = totalSuccessRate - playerSuccessRate;
                //If the difference is 0 then it will not be added so we can continue
                if (diff > 0)
                {
                    List<string> recommendKeys = songsToRecommend.Keys.ToList();
                    foreach (string songKey in recommendKeys)
                    {
                        songsToRecommend.TryGetValue(songKey, out var curr);
                        if (diff > (curr[1] - curr[0]))
                        {
                            songsToRecommend.Remove(songKey);
                            songsToRecommend.Add(key, new float[] { playerSuccessRate, totalSuccessRate });
                            break;
                        }
                    }
                }
            }
            return await EmbedHandler.PrintRecommendedSongs(ch, songsToRecommend, name);
        }
    }
}
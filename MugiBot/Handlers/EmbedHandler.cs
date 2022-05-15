using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Discord;
using Discord.WebSocket;
using PartyBot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyBot.Handlers
{


    //Have the Print functions print out the songkey so you can use it
    public static class EmbedHandler
    {
        /* This file is where we can store all the Embed Helper Tasks (So to speak). 
             We wrap all the creations of new EmbedBuilder's in a Task.Run to allow us to stick with Async calls. 
             All the Tasks here are also static which means we can call them from anywhere in our program. */
        public static async Task<Embed> CreateBasicEmbed(string title, string description, Color color)
        {
            if (description.Length > 2048)
            {
                return await CreateErrorEmbed("Description too long", "Is this still happening :worryconcon:");
            }
            var embed = await Task.Run(() => (new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color)
                .WithCurrentTimestamp().Build()));
            return embed;
        }

        public static async Task<Embed> TestingEmbedStuff()
        {
            var embed = await Task.Run(() => new EmbedBuilder()
                    .WithImageUrl("https://s4.anilist.co/file/anilistcdn/media/anime/cover/medium/bx20843-bk5OtUiP8htg.png")
                    .WithThumbnailUrl("https://i.ibb.co/VW3XFxT/Emilia-megumin.jpg")
                    .Build());
            return embed;
        }
        public static async Task<Embed> CreateErrorEmbed(string source, string error)
        {
            var embed = await Task.Run(() => new EmbedBuilder()
                .WithTitle($"ERROR OCCURED FROM - {source}")
                .WithDescription($"**Error Details**: \n{error}")
                .WithColor(Color.DarkRed)
                .WithCurrentTimestamp().Build());
            return embed;
        }

        public static async Task<Embed> OtherPlayerStats(ISocketMessageChannel ch, List<PlayerTableObject> playerObjects, string playerName)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder keys = new StringBuilder("\n All keys:\n");
            List<Embed> embeds = new List<Embed>();
            List<string> uniqueShows = new List<string>();
            float totalTimes = 0;
            float totalCorrect = 0;
            int count = 0;
            foreach (PlayerTableObject player in playerObjects)
            {
                totalTimes += player.TotalTimesPlayed;
                totalCorrect += player.TimesCorrect;
                sb.Append(SongTableObject.PrintSong(player) + $"\n\t Times Played: {player.TotalTimesPlayed} Times Correct: {player.TimesCorrect}\n\n");
                keys.Append(SongTableObject.MakeSongTableKey(player) + "\n");
                count++;
                if (!uniqueShows.Contains(player.Show))
                    uniqueShows.Add(player.Show);
                if (count % 10 == 0 || sb.Length + keys.Length > 1750)    
                    embeds = await AppendEmbedAndClear(embeds, sb, keys);
            }
            StringBuilder titleCard = new StringBuilder();
            titleCard.Append($"Success rate for this query: {Math.Round(totalCorrect / totalTimes, 3)}"
              + $"\n Total times correct: {totalCorrect} Total times played: {totalTimes}\n");
            titleCard.Append("Unique shows found:\n");
            foreach (string showFound in uniqueShows)
                titleCard.Append(showFound + "\n");

            return await PrintEmbeds(ch, embeds, sb, keys, titleCard);
        }

        // Need to convert all these things to use the String Builder function.
        public static async Task<Embed> PrintSongs(ISocketMessageChannel ch, List<SongTableObject> songObjects, bool printLinks = false)
        {
            var sb = new StringBuilder();
            var allKeys = new StringBuilder("\n All keys:\n");
            var embeds = new List<Embed>();
            var uniqueShows = new List<string>();
            int num = songObjects.Count;
            int count = 0;
            for (int i = 0; i < num; i++)
            {
                count++;
                if (sb.Length + allKeys.Length + LengthOfMessageContent(printLinks, songObjects[i]) > 2018)
                    embeds = await AppendEmbedAndClear(embeds, sb, allKeys);
                sb.Append($"{SongTableObject.PrintSong(songObjects[i])}\n\n");
                if (printLinks)
                {
                    // This could be done in a much smarter way.
                    if (songObjects[i].MP3 != null)
                        sb.Append($"MP3 {songObjects[i].MP3}\n");
                    if (songObjects[i]._720 != null)
                        sb.Append($"720 {songObjects[i]._720}\n");
                    if (songObjects[i]._480 != null)
                        sb.Append($"480 {songObjects[i]._480}\n");
                    sb.Append("\n");
                }
                allKeys.Append($"{songObjects[i].Key}\n");
                if (!uniqueShows.Contains(songObjects[i].Show))
                    uniqueShows.Add(songObjects[i].Show); 
                if ( (count % 10) == 0 )
                    embeds = await AppendEmbedAndClear(embeds, sb, allKeys);
            }

            StringBuilder titleCard = new StringBuilder();
            if (num > 150)
            {
                titleCard.Append("That is a lot of songs, I am not gonna print anything here.");
                return await PrintEmbeds(ch, embeds, sb, allKeys, titleCard);
            }
            titleCard.Append($"Found {num} songs for this query.\n\n");
            titleCard.Append("Unique shows found:\n");
            foreach (string showFound in uniqueShows)
                titleCard.Append(showFound + "\n");

            return await PrintEmbeds(ch, embeds, sb, allKeys, titleCard);
        }

        public static int LengthOfMessageContent(bool printLinks, SongTableObject song)
        {
            int value = 0;
            value += song.PrintSong().Length;
            value += song.Key.Length;
            if (printLinks)
            {
                if(song.MP3 != null)
                    value += song.MP3.Length;
                if(song._720 != null)
                    value += song._720.Length;
                if(song._480 != null)
                    value += song._480.Length;
            }
            return value;
        }

        public static async Task<Embed> PrintRecommendedSongs(ISocketMessageChannel ch,
            Dictionary<string, float[]> songsToRecommend, string name, Dictionary<string, string> songsToKeys)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder allKeys = new StringBuilder("\n All keys:\n");
            var list = songsToRecommend.Keys.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                songsToRecommend.TryGetValue(list[i], out var curr);
                sb.Append(list[i] + $"\n-  Total: {Math.Round(curr[1], 3)}  -  {name}: {Math.Round(curr[0], 3)}\n\n");
                allKeys.Append($"\n{songsToKeys[list[i]]}");
                if (i == list.Count - 1)
                    return await CreateBasicEmbed($"Recommendations for {name}", $"{sb.ToString()} {allKeys.ToString()}", Color.Blue);
                if (((i + 1) % 10) == 0)
                {
                    var message = await ch.SendMessageAsync(embed: await CreateBasicEmbed($"Recommendations for {name}", $"{sb.ToString()} {allKeys.ToString()}", Color.Blue));
                    sb.Clear();
                    allKeys.Clear();
                }
            }
            return await CreateBasicEmbed($"Recommendations for {name}", $"{sb.ToString()} {allKeys.ToString()}", Color.Blue);
        }

        // Iterates through an artist stats dictionary to print out all the stats for an artist
        public static async Task<Embed> PrintArtistStats(ISocketMessageChannel ch, Dictionary<string, int[]> artistStats, Dictionary<string, string> keysToSongs)
        {
            StringBuilder sb = new StringBuilder();
            var allKeys = new StringBuilder("\n All Keys: \n");
            var list = artistStats.Keys.ToList();
            var embeds = new List<Embed>();
            float[] totals = new float[] { 0, 0 };
            for (int i = 0; i < list.Count; i++)
            {
                artistStats.TryGetValue(list[i], out var curr);
                sb.Append(keysToSongs[list[i]] + $"\n-  Total Success: {Math.Round((float)curr[0] / curr[1], 3)}  -  Times Played: {curr[1]} -  Times Correct: {curr[0]}\n\n");
                allKeys.Append($"\n{list[i]}");
                totals[0] += artistStats[list[i]][0];
                totals[1] += artistStats[list[i]][1];
                if (i + 1 % 10 == 0 || (sb.Length + allKeys.Length) > 1850)
                    embeds = await AppendEmbedAndClear(embeds, sb, allKeys);
            }
            StringBuilder titleCard = new StringBuilder();
            titleCard.Append($"Found {artistStats.Count} songs for this query.\n\n");
            titleCard.Append($"Total success rate for this artist: {Math.Round(totals[0] / totals[1], 3)} \n"
              + $"\n Total times correct: {totals[0]} Total times played: {totals[1]}\n");
            if (Math.Round(totals[0] / totals[1], 3) < .333)
                titleCard.Append("Must be a tough one 😨\n");

            return await PrintEmbeds(ch, embeds, sb, allKeys, titleCard);
        }

        private static async Task<Embed> PrintEmbeds(ISocketMessageChannel ch, List<Embed> embeds, StringBuilder sb, StringBuilder allKeys, StringBuilder titleCard)
        {
            // Later I can use the cover images here and give the best and worst song from the query
            await ch.SendMessageAsync(embed: await CreateBasicEmbed($"Data, Search", titleCard.ToString(), Color.Blue));
            for (int i = 0; i < embeds.Count; i++)
            {
                await ch.SendMessageAsync(embed: embeds[i]);
            }
            try
            {
                return await CreateBasicEmbed($"Data, Search", $"{sb.ToString()} {allKeys.ToString()}", Color.Blue);
            }
            catch (Exception ex)
            {
                return await CreateErrorEmbed("Data, Search",
                "Something went wrong trying to print the ouput." + ex.Message);
            }
        }

        private static async Task<List<Embed>> AppendEmbedAndClear(List<Embed> embeds, StringBuilder sb, StringBuilder allKeys)
        {
            var embed = await CreateBasicEmbed($"Data, Search", $"{sb.ToString()} {allKeys.ToString()}", Color.Blue);
            embeds.Add(embed);
            sb.Clear();
            allKeys.Clear();
            allKeys.Append("\n All Keys: \n");
            return embeds;
        }
    }
}

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
            var embed = await Task.Run(() => (new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color)
                .WithCurrentTimestamp().Build()));
            return embed;
        }


        public static async Task<Embed> TestingEmbedStuff(){
            
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
                keys.Append(SongTableObject.MakeSongTableKey(player)+"\n");
                count++;
                if (!uniqueShows.Contains(player.Show))
                    uniqueShows.Add(player.Show);
                if (count % 10 == 0)
                {
                    var embed = await CreateBasicEmbed($"{playerName}'s Stats", $"{sb.ToString()} {keys.ToString()}", Color.Blue);
                    embeds.Add(embed);
                    sb.Clear();
                    keys.Clear();
                    keys.Append($"All keys:\n");
                }
            }
            StringBuilder titleCard = new StringBuilder();
            titleCard.Append($"Success rate for this query: {Math.Round(totalCorrect/totalTimes, 3)}"
              + $"\n Total times correct: {totalCorrect} Total times played: {totalTimes}\n");
            titleCard.Append("Unique shows found:\n");
            foreach (string showFound in uniqueShows)
                titleCard.Append(showFound + "\n");
            
            // Later I can use the cover images here and give the best and worst song from the query
            await ch.SendMessageAsync(embed: await CreateBasicEmbed($"{playerName}'s Stats", titleCard.ToString(), Color.Blue));
            for (int i = 0; i < embeds.Count; i++)
            {
                await ch.SendMessageAsync(embed: embeds[i]);
            }
            try
            {
                return await CreateBasicEmbed($"{playerName}'s Stats", $"{sb.ToString()} {keys.ToString()}", Color.Blue);
            }
            catch (Exception ex)
            {
                return await CreateBasicEmbed("Data, Search",
                "That is a lot of songs, please try and be more specific. Try typing the name of the exact season." + ex.Message, Color.Blue);
            }
        }

        // Need to convert all these things to use the String Builder function.
        public static async Task<Embed> PrintSongs(ISocketMessageChannel ch, List<SongTableObject> songObjects, bool printLinks = false)
        {
            int count = 0;
            StringBuilder sb = new StringBuilder();
            StringBuilder allKeys = new StringBuilder("\n For !playkey use the corresponding key: \n");
            List<Embed> embeds = new List<Embed>();
            List<string> uniqueShows = new List<string>();
            for (int i = 0; i < songObjects.Count; i++)
            {
                sb.Append($"{SongTableObject.PrintSong(songObjects[i])}\n\n");
                if (songObjects[i].MP3 != null && printLinks)
                    sb.Append($"\n\t MP3 {songObjects[i].MP3} ");
                if (songObjects[i]._720 != null && printLinks)
                    sb.Append($"\n 720 {songObjects[i]._720} ");
                if (songObjects[i]._480 != null && printLinks)
                    sb.Append($"\n 480 {songObjects[i]._480} ");
                allKeys.Append($"{songObjects[i].Key}\n");
                count++;
                if (!uniqueShows.Contains(songObjects[i].Show))
                    uniqueShows.Add(songObjects[i].Show);
                if (count % 10 == 0 || sb.Length > 1900)
                {
                    var embed = await CreateBasicEmbed($"Data, Search", $"{sb.ToString()} {allKeys.ToString()}", Color.Blue);
                    embeds.Add(embed);
                    sb.Clear();
                    allKeys.Clear();
                    allKeys.Append($"All keys:\n");
                }
            }
            StringBuilder titleCard = new StringBuilder();
            titleCard.Append($"Found {songObjects.Count} songs for this query.\n\n");
            titleCard.Append("Unique shows found:\n");
            foreach (string showFound in uniqueShows)
                titleCard.Append(showFound + "\n");
            
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
                return await CreateBasicEmbed("Data, Search",
                "That is a lot of songs, please try and be more specific. Try typing the name of the exact season." + ex.Message, Color.Blue);
            }
        }

        public static async Task<Embed> PrintRecommendedSongs(ISocketMessageChannel ch,
            Dictionary<string, float[]> songsToRecommend, string name, Dictionary<string,string> songsToKeys)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder allKeys = new StringBuilder("All Keys:");
            int count = 0;
            var list = songsToRecommend.Keys.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                songsToRecommend.TryGetValue(list[i], out var curr);
                sb.Append(list[i] + $"\n-  Total: {Math.Round(curr[1], 3)}  -  {name}: {Math.Round(curr[0], 3)}\n\n");
                allKeys.Append($"\n{songsToKeys[list[i]]}");
                count++;
                if (count % 10 == 0 && count == list.Count - 1)
                {
                    return await CreateBasicEmbed($"Recommendations for {name}", $"{sb.ToString()} {allKeys.ToString()}", Color.Blue);
                }
                if (count % 10 == 0)
                {
                    var message = await ch.SendMessageAsync(embed: await CreateBasicEmbed($"Recommendations for {name}", $"{sb.ToString()} {allKeys.ToString()}", Color.Blue));
                    sb.Clear();
                    allKeys.Clear();
                }
            }
            return await CreateBasicEmbed($"Recommendations for {name}", $"{sb.ToString()} {allKeys.ToString()}", Color.Blue);
        }

    }
}

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

            var embed1 = await Task.Run(() => new EmbedBuilder()
                    .WithImageUrl("https://i.ibb.co/VW3XFxT/Emilia-megumin.jpg")
                    .WithThumbnailUrl("https://wallpapers.com/images/high/rider-of-black-hd-wallpaper-rhgylnd0bxhutg4d.jpg")
                    .Build());
            
            var embed = await Task.Run(() => new EmbedBuilder()
                    .WithImageUrl("https://wallpapers.com/images/high/rider-of-black-hd-wallpaper-rhgylnd0bxhutg4d.jpg")
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

        public static async Task<Embed> PrintPlayerStats(List<PlayerTableObject> playerObjects, string playerName)
        {
            Dictionary<string, float> objectsToPrint = new Dictionary<string, float>();
            float totalTimes = 0;
            float totalCorrect = 0;
            foreach (PlayerTableObject player in playerObjects)
            {
                totalTimes += player.TotalTimesPlayed;

                totalCorrect += player.TimesCorrect;

                string temp = $"{player.Show} {player.Type} {player.SongName}";

                if (objectsToPrint.Count < 10)
                    objectsToPrint.Add(temp, (float)(player.TimesCorrect / player.TotalTimesPlayed));

                else if (objectsToPrint.Values.All(x => x < (float)(player.TimesCorrect / player.TotalTimesPlayed)))
                {
                    objectsToPrint.Remove(objectsToPrint.Max().Key);
                    objectsToPrint.Add(temp, (float)(player.TimesCorrect / player.TotalTimesPlayed));
                }
            }
            string list = $"{playerName}: Total Success Rate for this query: {totalCorrect / totalTimes} \n";
            foreach (string key in objectsToPrint.Keys)
                list += $"{key} \n \t Success Rate: {objectsToPrint[key]}\n";
            try
            {
                return await CreateBasicEmbed("Data, Search", list, Color.Blue);
            }
            catch (Exception ex)
            {
                return await CreateBasicEmbed("Data, Search",
                 "That is a lot of songs, please try and be more specific. Try typing the name of the exact season." + ex.Message, Color.Blue);
            }
        }

        public static async Task<Embed> OtherPlayerStats(ISocketMessageChannel ch, List<PlayerTableObject> playerObjects, string playerName)
        {
            string list = "";
            int count = 0;
            foreach (PlayerTableObject player in playerObjects)
            {
                //list = list + player.Key + " played: " +
                //player.TotalTimesPlayed + " correct: " + player.TimesCorrect + "\n";
                list = list + player.Show + " " + player.Type + " " + player.SongName + " by " + player.Artist +
                    "\n" + "\t" + " Times Played: " + player.TotalTimesPlayed + " Times Correct: " + player.TimesCorrect + "\n";
                count++;
                if (count % 10 == 0)
                {
                    await ch.SendMessageAsync(embed: await CreateBasicEmbed($"{playerName}'s Stats", list, Color.Blue));
                    list = "";
                }
            }
            try
            {
                return await CreateBasicEmbed("Data, Search", list, Color.Blue);
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
            string toPrint = "";
            int count = 0;
            for (int i = 0; i < songObjects.Count; i++)
            {
                toPrint += SongTableObject.PrintSong(songObjects[i]);
                if (songObjects[i].MP3 != null && printLinks)
                    toPrint += $"\n\t MP3 {songObjects[i].MP3} ";
                if (songObjects[i]._720 != null && printLinks)
                    toPrint += $"\n720 {songObjects[i]._720} ";
                if (songObjects[i]._480 != null && printLinks)
                    toPrint += $"\n480 {songObjects[i]._480} ";
                toPrint += $"\n Key for this song: {songObjects[i].Key}\n";
                count++;
                if (count % 10 == 0 && count == songObjects.Count - 1)
                {
                    return await CreateBasicEmbed("Data, Recommendation", toPrint, Color.Blue);
                }
                if (count % 10 == 0)
                {
                    await ch.SendMessageAsync(embed: await CreateBasicEmbed("Data, Recommendation", toPrint, Color.Blue));
                    toPrint = "";
                }
            }
            return await CreateBasicEmbed("Data, Search", toPrint, Color.Blue);
        }

        public static async Task<Embed> PrintRecommendedSongs(ISocketMessageChannel ch,
            Dictionary<string, float[]> songsToRecommend, string name)
        {
            string toPrint = "";
            int count = 0;
            var list = songsToRecommend.Keys.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                songsToRecommend.TryGetValue(list[i], out var curr);
                toPrint += list[i] + $"\n\t Total: {Math.Round(curr[1], 3)}  {name}: {Math.Round(curr[0], 3)}\n";
                count++;
                if (count % 10 == 0 && count == list.Count - 1)
                {
                    return await CreateBasicEmbed("Data, Recommendation", toPrint, Color.Blue);
                }
                if (count % 10 == 0)
                {
                    var message = await ch.SendMessageAsync(embed: await CreateBasicEmbed("Data, Recommendation", toPrint, Color.Blue));
                    toPrint = "";
                }
            }
            return await CreateBasicEmbed("Data, Recommendation", toPrint, Color.Blue);
        }

    }
}

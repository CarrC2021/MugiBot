using Discord;
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
    public class DBManager
    {
        private readonly AMQDBContext _db;
        public readonly PlayersRulesService _rs;
        private readonly char separator = Path.DirectorySeparatorChar;
        private readonly string mainpath;

        public readonly string JsonFiles;
        public readonly string ArchivedFiles;

        private readonly Dictionary<int, string> TypeConversion = new Dictionary<int, string>(){
                {1, "Opening"},
                {2, "Ending"},
                {3, "Insert Song"}
            };

        public DBManager(AMQDBContext database, PlayersRulesService rulesService)
        {
            _db = database;
            _rs = rulesService;
            // For now this is how the bot figures out its pathing. This is just pointing it to
            // the correct directories regardless of platform or run method.
            mainpath = Path.GetDirectoryName(System.Reflection.
            Assembly.GetExecutingAssembly().GetName().CodeBase).Replace($"{separator}bin{separator}Debug{separator}netcoreapp3.1", "").Replace($"file:{separator}", "");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                mainpath = separator + mainpath;
            JsonFiles = Path.Combine(mainpath, "LocalJson");
            ArchivedFiles = Path.Combine(mainpath, "archivedJsons");
        }

        /// <summary>
        /// This is an asynchronous function that iterates through all the json 
        /// files the bot has downloaded. It adds any new songs it finds 
        /// and new PlayerTableObjects when the player hears the song for the first time,
        /// look at <see cref="PartyBot.Database.PlayerTableObject"> for reference.
        /// It will also increment data in the PlayerTableObjects if the player has heard the song before.
        /// <summary>
        /// <returns> an <Embed> that the bot will print out once the asynchronous task is completed. </returns>
        public async Task<Embed> AddAllToDatabase()
        {
            FileInfo[] names = await JsonHandler.GetAllJsonInFolder(JsonFiles);
            foreach (FileInfo s in names)
            {
                bool songsOnly = false;
                Console.WriteLine(s.Name);

                // Check if this file should only add songs to the database, or if we want to also
                // track the player statistics contained in this file.
                if (s.Name.ToLower().Contains("teams") || s.Name.ToLower().Contains("co-op") || s.Name.ToLower().Contains("coop"))
                    songsOnly = true;
                await AddToDatabase(s.Name, songsOnly);
                // After the file has been used we can move it to archived files
                File.Move(Path.Combine(JsonFiles, s.Name), Path.Combine(ArchivedFiles, s.Name), true);
            }
            return await EmbedHandler.CreateBasicEmbed("Data", "All songs from the Json Files have been added to the Database and player stats were updated." 
                + $"\n\t There are now {await _db.SongTableObject.AsAsyncEnumerable().CountAsync()} songs in the database.", Color.Blue);
        }

        /// <summary>
        /// This is an asynchronous function that iterates through a List of strings which are file paths
        /// to json files that can be converted to a list of SongListData objects.
        /// To see this object's structure go to <see cref="PartyBot.DataStructs.SongListData"/>.
        /// It then iterates through a list of these SongListData objects and adds any song that is not in the 
        /// database currently.
        /// <summary>
        /// <param name="jsonFiles">.</param>
        /// <returns> an <Embed> once the asynchronous task is completed. </returns>
        public async Task<Embed> AddSongListFilesToDataBase(List<string> jsonFiles)
        {
            List<SongListData> data = await JsonHandler.ConvertSongJsons(jsonFiles);
            foreach (SongListData song in data)
            {
                // If the song links are null or the annId is 0 then there is no point in adding it so just
                // continue on to the next song.
                if (song.LinkMp3 == null || song.annId == 0)
                    continue;

                var query = await _db.SongTableObject.FindAsync(SongTableObject.MakeSongTableKey(song.annId, song.type, song.songName, song.artist));
               //If the song was not found in the database then we want to add it
                if (query == null)
                {
                    SongTableObject temp = SongTableObject.SongListDataToTable(song);
                    await _db.AddAsync(temp);
                }
            }
            await _db.SaveChangesAsync();
            return await EmbedHandler.CreateBasicEmbed("Data", "All songs from the Json Files have been added to the Database."
                + $"\n\t There are now {await _db.SongTableObject.AsAsyncEnumerable().CountAsync()} songs in the database.", Color.Blue);
        }


        public async Task AddToDatabase(string filename, bool songsOnly)
        {
            // Get the directpath to the file and then convert the contents to a List of SongData objects.
            string filepath = Path.Combine(JsonFiles, filename);
            List<SongData> data = await JsonHandler.ConvertJsonToSongData(new FileInfo(filepath));
            // Get the rules we want to check and get the players we are tracking.
            var playerDict = await _rs.GetPlayersTracked();
            foreach (SongData song in data)
            {
                // This indicates that a new game has begun. Since there is a chance that the
                // same song is played we need to update the database. 
                if (song.songNumber == 1)
                    await _db.SaveChangesAsync();

                if (song.urls == null)
                    continue;

                // Update the songs since the urls are there.
                var query = await _db.SongTableObject.FindAsync(SongTableObject.MakeSongTableKey(song.annId, song.type, song.name, song.artist));
                // If this song is not found in the database then we need to create a tableobject and add it.
                if (query == null)
                {
                    SongTableObject temp = SongTableObject.SongDataToSongTableObject(song);
                    await _db.AddAsync(temp);
                }
                if (songsOnly)
                    continue;
                // Update the player stats when songsOnly is false.
                await UpdatePlayerStats(song, playerDict);
            }
            await _db.SaveChangesAsync();
        }

        private async Task UpdatePlayerStats(SongData song, Dictionary<string, string> playerDict)
        {
            Dictionary<string, int> listStatusDict = new Dictionary<string, int>();
            var RulesMetList = await _rs.RulesMetBySongData(song, playerDict);
            RulesMetList.Add("");
            // If the game is not a ranked game then we want to update everyone's list status.
            if (!song.gameMode.Equals("Ranked"))
            {
                foreach (Fromlist listInfo in song.fromList)
                    listStatusDict.Add(listInfo.name, listInfo.listStatus);
            }
            int listnum;
            foreach (string rule in RulesMetList)
            {
                foreach (Player player in song.players)
                {
                    // If the player is not in the json of players to track then continue to the next player.
                    if (!playerDict.ContainsKey(player.name))
                        continue;
                    try
                    {
                        // Try to find the player's list status from the game, if you cannot find it then it stays as 0.
                        listnum = 0;
                        if (listStatusDict != null && listStatusDict.ContainsKey(player.name))
                            listnum = listStatusDict[player.name];

                        PlayerTableObject query = await _db.PlayerStats.FindAsync(PlayerTableObject.MakePlayerTableKey
                            (song.annId, song.type, song.name, song.artist, playerDict[player.name], rule));
                        // If you cannot find the PlayerTableObject then add it to the database.
                        if (query == null)
                        {
                            await _db.AddAsync(new PlayerTableObject(SongTableObject.SongDataToSongTableObject(song),
                             playerDict[player.name], listnum, player.correct, rule));
                        }
                        // If you found the PlayerTableObject then increment the stats.
                        else
                        {
                            query.FromList = listnum;
                            query.Increment(player.correct);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task<Embed> MergeTest(string mergeFrom, string mergeInto)
            => await DBMergeHandler.MergePlayers(_db, mergeFrom, mergeInto);

        // This function is used to update the song database using a json that is created by exporting from the expand library.
        public async Task<Embed> UpdateSongDatabase(string expandLibraryFile)
        {
            AMQExpandData data = await JsonHandler.ConvertJsonToAMQExpandData(new FileInfo(Path.Combine(mainpath, expandLibraryFile)));
            foreach (Question question in data.Questions)
                await AddSongsFromQuestion(question);

            await _db.SaveChangesAsync();
            return await EmbedHandler.CreateBasicEmbed("Data, Songs", $"There are now {await _db.SongTableObject.AsAsyncEnumerable().CountAsync()} songs.", Color.Blue);
        }

        // This function updates the database using the file you receive when exporting all songs from expand library.
        private async Task AddSongsFromQuestion(Question question)
        {
            foreach (Song song in question.Songs)
            {
                string Type = song.Number > 0 ? $"{TypeConversion[song.Type]} {song.Number}" : $"{TypeConversion[song.Type]}";
                var result = await _db.SongTableObject.FindAsync(SongTableObject.MakeSongTableKey(question.AnnId, Type, song.Name, song.Artist));
                if (result == null)
                {
                    try
                    {
                        await _db.SongTableObject.AddAsync(new SongTableObject(song.Name, song.Artist, Type,
                        question.Name, "", song.Examples.Mp3, question.AnnId, song.Examples._720, song.Examples._480, song.AnnSongId));
                    }
                    catch (Exception ex)
                    {
                        await LoggingService.LogAsync(ex.Source, LogSeverity.Error, ex.Message);
                    }
                }
                else
                {
                    result.AnnSongID = song.AnnSongId;
                }
            }
        }

        public async Task<Embed> RemoveDeadSongs()
        {
            var toRemove = await _db.SongTableObject
                    .AsTracking()
                    .Where(f => f.AnnID == 0)
                    .ToListAsync();

            _db.RemoveRange(toRemove);  
            await _db.SaveChangesAsync();

            var alsoToRemove = await _db.SongTableObject
                    .AsTracking()
                    .Where(k => k.Key.Contains("AnnID "))
                    .ToListAsync();
            _db.RemoveRange(alsoToRemove);  
            await _db.SaveChangesAsync();
            
            return await EmbedHandler.CreateBasicEmbed("Data, Songs", $"There are now {await _db.SongTableObject.AsAsyncEnumerable().CountAsync()} songs.", Color.Blue);            
        }

    }
}

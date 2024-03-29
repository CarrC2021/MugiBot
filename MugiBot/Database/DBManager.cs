﻿using Discord;
using Discord.Commands;
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
    public class DBManager
    {
        public readonly PlayersRulesService _rs;
        private readonly char separator = Path.DirectorySeparatorChar;
        public string mainpath { get; set; }
        public string JsonFiles { get; set; }
        public string ArchivedFiles { get; set; }
        public List<ulong> DatabaseAdminIds { get; set; }
        public readonly AnimeRelationManager animeRelationManager;

        private readonly Dictionary<int, string> TypeConversion = new Dictionary<int, string>(){
                {1, "Opening"},
                {2, "Ending"},
                {3, "Insert Song"}
            };

        public DBManager(AMQDBContext database, PlayersRulesService rulesService)
        {
            _rs = rulesService;
            DatabaseAdminIds = new List<ulong>();
            animeRelationManager = new AnimeRelationManager();
        }

        public void SetSubPaths()
        {
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
            using var _db = new AMQDBContext();
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
                await Task.Run( () => File.Move(Path.Combine(JsonFiles, s.Name), Path.Combine(ArchivedFiles, s.Name), true));
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
            using var _db = new AMQDBContext();
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
            using var _db = new AMQDBContext();
            await animeRelationManager.UpdateRelationalMapAsync(data);
            foreach (SongData song in data)
            {
                // This indicates that a new game has begun. Since there is a chance that the
                // same song is played we need to update the database. 
                if (song.songNumber == 1)
                    await _db.SaveChangesAsync();

                if (song.annId.Equals(null) || song.urls.catbox._0 == null || song.urls.catbox._0.Equals("") || song.annId <= 0)
                    continue;

                // Update the songs since the urls are there.
                var query = await _db.SongTableObject.FindAsync(SongTableObject.MakeSongTableKey(song.annId, song.type, song.name, song.artist));
                // If this song is not found in the database then we need to create a tableobject and add it.
                if (query == null)
                {
                    SongTableObject temp = SongTableObject.SongDataToSongTableObject(song);
                    await _db.AddAsync(temp);
                }
                // If it is not null then update the titles of the show.
                else
                {
                    query.Show = song.anime.english;
                    query.Romaji = song.anime.romaji;
                    query._720 = song.urls.catbox._720;
                    query._480 = song.urls.catbox._480;
                    query.MP3 = song.urls.catbox._0;
                }
                // If not updating player statistics then continue through
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
            using var _db = new AMQDBContext();
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
                        await LoggingService.LogAsync(ex.Source, LogSeverity.Error, ex.Message);
                    }
                }
            }
        }

        public async Task<Embed> MergeTest(string mergeFrom, string mergeInto)
            => await DBMergeHandler.MergePlayers(mergeFrom, mergeInto);

        // This function is used to update the song database using a json that is created by exporting from the expand library.
        public async Task<Embed> UpdateSongDatabase(SocketUser user, string expandLibraryFile)
        {
            using var _db = new AMQDBContext();
            if(!DatabaseAdminIds.Contains(user.Id))
                return await EmbedHandler.CreateErrorEmbed("Data, Songs", $"You do not have the privileges necessary to use this method.");
            List<Question> data = await JsonHandler.ConvertJsonToAMQExpandData(new FileInfo(Path.Combine(mainpath, expandLibraryFile)));
            foreach (Question question in data)
            {
                //await LoggingService.LogInformationAsync("expand data", $"AnnID: {question.AnnId} and Show name: {question.Name}");
                await AddSongsFromQuestion(question);
            }

            await _db.SaveChangesAsync();
            await Task.Run(() => File.Delete(Path.Combine(mainpath, expandLibraryFile)));
            return await EmbedHandler.CreateBasicEmbed("Data, Songs", $"There are now {await _db.SongTableObject.AsAsyncEnumerable().CountAsync()} songs.", Color.Blue);
        }

        // This function updates the database using the file you receive when exporting all songs from expand library.
        private async Task AddSongsFromQuestion(Question question)
        {
            await animeRelationManager.UpdateRelationalMap(question.AnnId, question.Name);
            using var _db = new AMQDBContext();
            foreach (Song song in question.Songs)
            {
                if (song.Examples.Mp3 == null  || song.Examples.Mp3.Equals(""))
                    continue;
                string Type = song.Number > 0 ? $"{TypeConversion[song.Type]} {song.Number}" : $"{TypeConversion[song.Type]}";
                // Need to work on incorporating the artist ID for this not to be a nightmare.
                var result = await _db.SongTableObject.FindAsync(SongTableObject.MakeSongTableKey(question.AnnId, Type, song.Name, song.Artist));
                if (result == null)
                {
                    try
                    {
                        await _db.SongTableObject.AddAsync(new SongTableObject(song.Name, song.Artist, Type,
                        question.Name, "", song.Examples.Mp3, question.AnnId, song.Examples._720, song.AnnSongId));
                    }
                    catch (Exception ex)
                    {
                        await LoggingService.LogAsync(ex.Source, LogSeverity.Error, ex.Message);
                    }
                }
                // Update the links just in case a link has been changed.
                else
                {
                    result.AnnSongID = song.AnnSongId;
                    result._720 = song.Examples._720;
                    result.MP3 = song.Examples.Mp3;
                    result.Show = question.Name;
                }
            }
            await _db.SaveChangesAsync();
        }

        public async Task<Embed> UpdateSongLink(string songKey, string newLink, ulong messengerID)
        {
            if (!DatabaseAdminIds.Contains(messengerID))
                return await EmbedHandler.CreateErrorEmbed("Database", "You do not have permission to update links.");
            if (!newLink.ToLower().Contains("catbox") || !newLink.ToLower().EndsWith(".mp3"))
                return await EmbedHandler.CreateErrorEmbed("Database", "This link does not look correct.");
            var tableObject = await DBSearchService.UseSongKey(songKey);
            if (tableObject != null)
                tableObject.MP3 = newLink;
            return await EmbedHandler.CreateBasicEmbed("Database", $"The link for {tableObject.PrintSong()} has been updated to {newLink}." +
            " Contact mods if this looks incorrect. ", Color.Blue);
        }

        public async Task<Embed> RemoveDeadSongs()
        {
            using var _db = new AMQDBContext();
            var toRemove = await _db.SongTableObject
                    .AsTracking()
                    .Where(f => f.Key.StartsWith("-1"))
                    .Where(f => f.AnnID <= 0)
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

        public string FixString(string toFix)
        {
            return toFix.Replace("ū","uu").Replace("ō","ou").Replace("Ō","Oo");
        }

    }
}

using Discord;
using PartyBot.DataStructs;
using PartyBot.Handlers;
using PartyBot.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PartyBot.Database
{
    public class DBManager
    {
        private readonly AMQDBContext _db;
        public readonly PlayersRulesService _rs;
        public readonly DBSearchService _search;
        private readonly char separator = Path.DirectorySeparatorChar;
        private readonly string mainpath;

        public readonly string JsonFiles;
        public readonly string ArchivedFiles;

        public async Task<Embed> MergeTest(string mergeFrom, string mergeInto)
            => await DBMergeHandler.MergePlayers(_db, mergeFrom, mergeInto);

        public DBManager(AMQDBContext database, PlayersRulesService rulesService)
        {
            _db = database;
            _rs = rulesService;
            mainpath = Path.GetDirectoryName(System.Reflection.
            Assembly.GetExecutingAssembly().GetName().CodeBase).Replace($"{separator}bin{separator}Debug{separator}netcoreapp3.1", "").Replace($"file:{separator}", "");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                mainpath = separator + mainpath;
            _search = new DBSearchService();
            JsonFiles = Path.Combine(mainpath, "LocalJson");
            ArchivedFiles = Path.Combine(mainpath, "archivedJsons");
        }
        public async Task<Embed> AddAllToDatabase()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            FileInfo[] names = await JsonHandler.GetAllJsonInFolder(JsonFiles);
            foreach (FileInfo s in names)
            {
                bool songsOnly = false;
                Console.WriteLine(s.Name);
                if (s.Name.ToLower().Contains("teams") || s.Name.ToLower().Contains("co-op") || s.Name.ToLower().Contains("coop"))
                {
                    songsOnly = true;
                    Console.WriteLine("Added only songs for this file" + s);
                }
                await AddToDatabase(_rs, s.Name, songsOnly);
            }
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;
            // Format and display the TimeSpan value.
            string elapsedTime = string.Format("{0:00}:{1:00}.{2:00}",
                ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            return await EmbedHandler.CreateBasicEmbed("Data", "All songs from the Json Files have" +
                " been added to the Database and player stats were updated \n" + "RunTime: " + elapsedTime
                + $"\n\t There are now {await _db.SongTableObject.AsAsyncEnumerable().CountAsync()} songs in the database.", Color.Blue);
        }

        public async Task<Embed> AddGithubFilesToDataBase(List<string> jsonFiles)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            List<SongListData> data = await JsonHandler.ConvertSongJsons(jsonFiles);
            foreach (SongListData song in data)
            {
                //update the songs since the urls are there
                if (song.LinkMp3 == null)
                    continue;
                var query = await _db.SongTableObject.FindAsync(SongTableObject.MakeSongTableKey(song.annId, song.type, song.songName, song.artist));
                //if it is not just that the show's name got changed in the database, then we want to add the new object
                if (query == null)
                {
                    SongTableObject temp = ConvertSongListDataToTable(song);
                    await _db.AddAsync(temp);
                }
                //await _db.SaveChangesAsync();
            }
            await _db.SaveChangesAsync();
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;
            // Format and display the TimeSpan value.
            string elapsedTime = string.Format("{0:00}:{1:00}.{2:00}",
                ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            return await EmbedHandler.CreateBasicEmbed("Data", "All songs from the Json Files have been added to the Database and"
                + $"player stats were updated \n RunTime: {elapsedTime}"
                + $"\n\t There are now {await _db.SongTableObject.AsAsyncEnumerable().CountAsync()} songs in the database.", Color.Blue);
        }

        public async Task AddToDatabase(PlayersRulesService _playersRulesService, string filename, bool songsOnly)
        {
            string filepath = Path.Combine(JsonFiles, filename);
            List<SongData> data = await JsonHandler.ConvertJsonToSongData(new FileInfo(filepath));
            List<string> rules = await _playersRulesService.GetRules();
            var playerDict = await _playersRulesService.GetPlayersTracked();
            foreach (SongData song in data)
            {
                if (song.songNumber == 1)
                    await _db.SaveChangesAsync();

                if (song.urls == null)
                    continue;

                //update the songs since the urls are there
                var query = await _db.SongTableObject.FindAsync(SongTableObject.MakeSongTableKey(song.annId, song.type, song.name, song.artist));
                //if this song is not found in the database then we need to create a tableobject and add it
                if (query == null)
                {
                    SongTableObject temp = ConvertSongDataToTable(song);
                    await _db.AddAsync(temp);
                }
                if (songsOnly)
                    continue;
                //update the player stats when songsOnly is false
                await UpdatePlayerStats(_playersRulesService, song, rules, playerDict);
            }
            await _db.SaveChangesAsync();
            File.Move(filepath, Path.Combine(ArchivedFiles, filename), true);
        }

        private async Task UpdatePlayerStats(PlayersRulesService _service, SongData song, List<string> rules, Dictionary<string, string> playerDict)
        {
            Dictionary<string, int> tempDict = new Dictionary<string, int>();
            var RulesMetList = await _service.RulesMetBySongData(song, rules, playerDict);
            RulesMetList.Add("");
            //If the game is not a ranked game then we want to update everyone's list status
            if (!song.gameMode.Equals("Ranked"))
            {
                foreach (Fromlist listInfo in song.fromList)
                    tempDict.Add(listInfo.name, listInfo.listStatus);
            }
            int listnum;
            foreach (string rule in RulesMetList)
            {
                foreach (Player player in song.players)
                {
                    //If the player is not in the json of players to track then continue to the next player
                    if (!playerDict.ContainsKey(player.name))
                        continue;
                    try
                    {
                        //Try to find the player's list status from the game, if you cannot set it to 0
                        listnum = 0;
                        if (tempDict != null && tempDict.ContainsKey(player.name))
                            listnum = tempDict[player.name];

                        PlayerTableObject query = await _db.PlayerStats.FindAsync(PlayerTableObject.MakePlayerTableKey
                            (song.annId, song.type, song.name, song.artist, playerDict[player.name], rule));
                        if (query == null)
                        {
                            await _db.AddAsync(new PlayerTableObject((song, playerDict[player.name], listnum, rule, player.correct));
                        }
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

        public async Task<Embed> UpdateLinks(string showkey, string videoLink, string mp3 = "default")
        {
            SongTableObject tableObject = await _db.SongTableObject.FindAsync(showkey);
            tableObject._720 = videoLink;
            tableObject.MP3 = mp3;
            await _db.SaveChangesAsync();
            return await EmbedHandler.CreateBasicEmbed("Data, Songs", $"Updated {showkey} with the links provided", Color.Blue);
        }

        private SongTableObject ConvertSongDataToTable(SongData songData)
        {
            return new SongTableObject(songData.name, songData.artist,
            songData.type, songData.anime.english, songData.anime.romaji, songData.urls.catbox._0,
            songData.annId, songData.urls.catbox._720, songData.urls.catbox._480);
        }

        private SongTableObject ConvertSongListDataToTable(SongListData songData)
        {
            return new SongTableObject(songData.songName, songData.artist,
            songData.type, songData.animeEng, songData.animeRomaji, songData.LinkMp3,
            songData.annId, songData.LinkVideo);
        }
    }

}

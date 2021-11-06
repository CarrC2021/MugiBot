using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PartyBot.DataStructs;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PartyBot.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using MyAnimeList.API;
using MyAnimeList.API.DTOs.Requests;
using System.IO;
using PartyBot.Handlers;
using Newtonsoft.Json;

namespace PartyBot.Database
{
    public class AnimeRelationManager
    {
        /// <summary>
        /// Takes an annID as input and adds a new AnimeRelationalMap to the database if that key is not present.
        /// <summary>
        /// <param name="annID">.</param>
        public async Task UpdateRelationalMap(int annID, string engName = "not known", string romaji = "not known")
        {
            using var db = new AMQDBContext();
            var mapResult = await db.AnimeRelationalMaps.FindAsync(annID);
            if (mapResult == null)
            {
                await db.AnimeRelationalMaps.AddAsync(new AnimeRelationalMap(annID, engName, romaji));
                Console.WriteLine($"{annID} show name {engName} or {romaji} has been added to the database.");
                await db.SaveChangesAsync();
            }
            else
            {
                mapResult.EngName = engName;
                mapResult.Romaji = romaji;
            }
        }

        public async Task GetAuthAsync()
        {
            string path = Path.Combine(GlobalData.Config.RootFolderPath, "malAPI.json");
            var returned = JsonConvert.DeserializeObject<UserParams>(await File.ReadAllTextAsync(path));
            var userParams = new UserParams
            {
                ClientId = returned.ClientId,
                OAuth2State = returned.OAuth2State,
                RedirectURI = returned.RedirectURI
            };
            var malClient = new MALApiClient(userParams);
            string code = await Task.Run(() => malClient.GetAuthUrl()); //The URL to authorize the API usage for our account
            Console.WriteLine(code); // Print to console, copy the url and allow from a browser
            malClient.DoAuth(Console.ReadLine()); // From the url copy the code and paste it.
            string s = Console.ReadLine();//Input string to search
            GetAnimeListRequest xd = new GetAnimeListRequest // Create a request object, limit is the maximum amount of entries to obtain
            {
                Search = "One",
                Limit = 10
            };
            Console.WriteLine("\n\n Test List:");
            var lRes = malClient.GetAnimeList(xd).Result;
            foreach (var item in lRes.data)
            {
                Console.WriteLine("\t" + item.node.title);
            }

            Console.WriteLine("\n\n Test Refresh Auth:");

            xd.Search = "Souma";

            lRes = malClient.GetAnimeList(xd).Result;
            foreach (var item in lRes.data)
            {
                Console.WriteLine("\t" + item.node.title);
            }
        }

        /// <summary>
        /// Updates the relational map Using the Song Table. Only use this to initialize the relational map.
        /// <summary>
        public async Task UpdateRelationalMapUsingSongTable()
        {
            using var db = new AMQDBContext();
            var songs = await db.SongTableObject.ToListAsync();
            //foreach (SongTableObject song in songs)
                //await UpdateRelationalMap(song.AnnID, song.Show, song.Romaji);
            //await db.SaveChangesAsync();
            foreach (SongTableObject song in songs)
            {
            }
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Takes an annID as input and adds a new AnimeRelationalMap to the database if that key is not present.
        /// <summary>
        /// <param name="annID">.</param>
        public async Task UpdateRelationalMapAsync(List<SongData> songs)
        {
            using var db = new AMQDBContext();
            foreach (SongData song in songs)
                await UpdateRelationalData(db, song);
        }

        /// <summary>
        /// Takes an annID as input and adds a new AnimeRelationalMap to the database if that key is not present.
        /// <summary>
        /// <param name="annID">.</param>
        public async Task UpdateRelationalData(AMQDBContext db, SongData song)
        {
            var mapResult = await db.AnimeRelationalMaps.FindAsync(song.annId);
            try
            {
                await AddToTable(db, mapResult, song);
            }
            catch (Exception ex)
            {
                await LoggingService.LogCriticalAsync(ex.Source, ex.Message);
            }
        }

        public async Task AddToTable(AMQDBContext db, AnimeRelationalMap mapResult, SongData song)
        {
            if (song.SiteIDs == null)
                return;
            if (mapResult == null)
            {
                await db.AnimeRelationalMaps.AddAsync(new AnimeRelationalMap(song.annId, song.anime.english, song.anime.romaji, song.SiteIDs.aniListId,
                song.SiteIDs.kitsuId, song.SiteIDs.malId));
            }
            else
            {
                mapResult.EngName = song.anime.english;
                mapResult.Romaji = song.anime.romaji;
                mapResult.MALID = song.SiteIDs.malId;
                mapResult.KitsuID = song.SiteIDs.kitsuId;
                mapResult.AnilistID = song.SiteIDs.aniListId;
            }
            await db.SaveChangesAsync();
        }
    }
}
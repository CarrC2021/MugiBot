using Discord;
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
    public class AnimeRelationManager
    {
        /// <summary>
        /// Takes an annID as input and adds a new AnimeRelationalMap to the database if that key is not present.
        /// <summary>
        /// <param name="annID">.</param>
        public async Task UpdateRelationalMap(int annID)
        {
            using var db = new AMQDBContext();
            var mapResult = await db.AnimeRelationalMaps.FindAsync(annID);
            if (mapResult == null)
            {
                await db.AnimeRelationalMaps.AddAsync(new AnimeRelationalMap(annID));
                await db.SaveChangesAsync();
            }
        }
        /// <summary>
        /// Takes an annID as input and adds a new AnimeRelationalMap to the database if that key is not present.
        /// <summary>
        /// <param name="annID">.</param>
        public async Task UpdateRelationalMap(SongData song)
        {
            using var db = new AMQDBContext();
            var mapResult = await db.AnimeRelationalMaps.FindAsync(song.annId);
            if (mapResult == null)
            {
                await db.AnimeRelationalMaps.AddAsync(new AnimeRelationalMap(song.annId, song.anime.english, song.anime.romaji, song.SiteIDs.aniListId,
                song.SiteIDs.kitsuId, song.SiteIDs.malId));
                await db.SaveChangesAsync();
            }
            else
            {
                mapResult.MALID = song.SiteIDs.malId;
                mapResult.KitsuID = song.SiteIDs.kitsuId;
                mapResult.AnilistID = song.SiteIDs.aniListId;
            }
        }
    }
}
﻿using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using PartyBot.Database;
using PartyBot.Services;
using PartyBot.Handlers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DBSearchService
{

    public static async Task<Embed> OtherPlayerStats(ISocketMessageChannel ch, string playerName, string showName, string exact = "no")
    {
        using var db = new AMQDBContext();
        var thing = await SearchHandler.PlayerStatsSearch(db, playerName, showName, "any", exact);
        return await EmbedHandler.OtherPlayerStats(ch, thing, playerName);
    }

    public static async Task<Embed> SearchForShow(SocketMessage message, string name, string type = "any", bool exactmatch = false, bool links = false)
    {
        using var db = new AMQDBContext();
        var songList = await SearchHandler.ShowSearch(name, type, exactmatch);
        if (links)
            return await EmbedHandler.PrintSongs(message.Channel, songList, true);
        return await EmbedHandler.PrintSongs(message.Channel, songList);
    }

    public static async Task<Embed> SearchByArtist(SocketMessage message, string author, string printLinks = "no", bool exact = false)
    {
        using var db = new AMQDBContext();
        var songList = await SearchHandler.SearchArtist(db, author, exact);
        if (printLinks.Equals("no"))
            return await EmbedHandler.PrintSongs(message.Channel, songList);
        return await EmbedHandler.PrintSongs(message.Channel, songList, true);
    }

    public static async Task<List<SongTableObject>> ReturnSongsByArtist(string artist, bool exact)
    {
        using var db = new AMQDBContext();
        return await SearchHandler.SearchArtist(db, artist, exact);
    }
    public static async Task<Embed> ListPlayerStats(ISocketMessageChannel channel, string playerName, string showName, PlayersRulesService rulesService, string type = "any", string exactMatch = "no")
    {
        var players = await rulesService.GetPlayersTracked();
        if (!players.Keys.Contains(playerName))
            return await EmbedHandler.CreateErrorEmbed("PlayerStats Error", "Found no one in the database with that username.");
        using var db = new AMQDBContext();
        var playerObjects = await SearchHandler.PlayerStatsSearch(db, playerName, showName, type, exactMatch);
        return await EmbedHandler.OtherPlayerStats(channel, playerObjects, playerName);
    }

    public static async Task<Embed> PlayerStatsByArtist(ISocketMessageChannel message, string playerName, string artist, string type = "any", string exactMatch = "no")
    {
        using var db = new AMQDBContext();
        var playerObjects = await SearchHandler.PlayerStatsSearchByArtist(db, playerName, artist, type, exactMatch);
        return await EmbedHandler.OtherPlayerStats(message, playerObjects, playerName);
    }

    public static async Task<Embed> StatsByArtist(ISocketMessageChannel message, string artist, string type = "any", bool exactMatch = false)
    {
        using var db = new AMQDBContext();
        var playerObjects = await SearchHandler.StatsSearchArtist(db, artist, type, exactMatch);
        var artistStats = DBCalculationHandler.ArtistStatsFromList(playerObjects);
        return await EmbedHandler.PrintArtistStats(message, artistStats.Item1, artistStats.Item2);
    }

    public static async Task<SongTableObject> UseSongKey(string key)
    {
        return await SearchHandler.UseSongKey(key);
    }

    public static async Task<List<PlayerTableObject>> ReturnAllPlayerObjects()
    {
        using var db = new AMQDBContext();
        return await db.PlayerStats
            .AsNoTracking()
            .ToListAsync();
    }
    public static async Task<List<PlayerTableObject>> ReturnAllPlayerObjectsByType(string type)
    {
        using var db = new AMQDBContext();
        return await db.PlayerStats
        .AsNoTracking()
        .Where(j => j.Type.ToLower().Contains(type.ToLower()))
        .ToListAsync();
    }
    public static async Task<List<PlayerTableObject>> ReturnAllPlayerObjects(string player)
    {
        using var db = new AMQDBContext();
        return await db.PlayerStats
            .AsNoTracking()
            .Where(f => f.PlayerName.ToLower().Equals(player.ToLower()))
            .ToListAsync();
    }
    public static async Task<List<PlayerTableObject>> ReturnAllPlayerObjects(string player, int list)
    {
        using var db = new AMQDBContext();
        return await db.PlayerStats
            .AsNoTracking()
            .Where(j => j.FromList == list)
            .Where(f => f.PlayerName.ToLower().Equals(player.ToLower()))
            .ToListAsync();
    }
    public static async Task<List<PlayerTableObject>> ReturnAllPlayerObjects(string player, string type)
    {
        using var db = new AMQDBContext();
        return await db.PlayerStats
            .AsNoTracking()
            .Where(j => j.Type.ToLower().Contains(type.ToLower()))
            .Where(f => f.PlayerName.ToLower().Equals(player.ToLower()))
            .ToListAsync();
    }
    public static async Task<List<PlayerTableObject>> ReturnAllPlayerObjects(string player, string type, int list)
    {
        using var db = new AMQDBContext();
        return await db.PlayerStats
            .AsNoTracking()
            .Where(j => j.Type.ToLower().Contains(type.ToLower()))
            .Where(j => j.FromList == list)
            .Where(f => f.PlayerName.ToLower().Equals(player.ToLower()))
            .ToListAsync();
    }
    public static async Task<List<PlayerTableObject>> ReturnAllPlayerObjects(string player, string type, int list, string rule)
    {
        using var db = new AMQDBContext();
        return await db.PlayerStats
            .AsNoTracking()
            .Where(j => j.Type.ToLower().Contains(type.ToLower()))
            .Where(j => j.FromList == list)
            .Where(f => f.PlayerName.ToLower().Equals(player.ToLower()))
            .Where(t => t.Rule.Equals(rule))
            .ToListAsync();
    }
    public static async Task<List<SongTableObject>> ReturnAllSongObjects()
    {
        using var db = new AMQDBContext();
        return await db.SongTableObject
            .AsNoTracking()
            .ToListAsync();
    }

    public static async Task<List<SongTableObject>> ReturnAllSongObjects(string show)
    {
        using var db = new AMQDBContext();
        return await db.SongTableObject
            .AsNoTracking()
            .Where(f => f.Show.ToLower().Equals(show.ToLower()))
            .ToListAsync();
    }
    public static async Task<List<SongTableObject>> ReturnAllSongObjectsByType(string type)
    {
        using var db = new AMQDBContext();
        return await db.SongTableObject
            .AsNoTracking()
            .Where(f => f.Type.ToLower().Contains(type.ToLower()))
            .ToListAsync();
    }

}

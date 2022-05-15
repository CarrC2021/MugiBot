using System.Text;
using Discord;
using Discord.WebSocket;
using PartyBot.Handlers;
using PartyBot.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using PartyBot.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PartyBot.DataStructs;

public class Radio
{
    public bool RadioMode;
    public List<string> CurrType;
    public List<string> CurrPlayers;
    private readonly List<List<string>> SongTypes;
    public readonly SocketGuild Guild;
    public readonly IMessageChannel Channel;
    public List<int> ListNums = new List<int>();
    public Dictionary<string, int> listStatus = new Dictionary<string, int>();
    public Dictionary<int, string> listStatusReverse = new Dictionary<int, string>();
    private List<SongTableObject> SongSelection = new List<SongTableObject>();
    private Queue<SongTableObject> Queue = new Queue<SongTableObject>();
    private readonly Random rnd;
    public Radio(IMessageChannel c, SocketGuild g)
    {
        rnd = new Random();
        Guild = g;
        Channel = c;
        CurrType = new List<string>{"Opening", "Ending"};
        CurrPlayers = new List<string>{"any"};
        ListNums.AddRange(new int[] { 1, 2, 3, 4, 5 });
        listStatus = new Dictionary<string, int>
        {
            { "watching", 1 },
            { "completed", 2 },
            { "paused", 3 },
            { "dropped", 4 },
            { "planning", 5 }
        };
        listStatusReverse = new Dictionary<int, string>()
        {
            { 1, "Watching" },
            { 2, "Completed" },
            { 3, "Paused" },
            { 4, "Dropped" },
            { 5, "Planning"}
        };
        List<List<string>> list = new List<List<string>> {new List<string>{"Opening"},
            new List<string>{"Opening", "Ending"}, new List<string>{"Ending"}, new List<string>{"Opening", "Ending", "Insert"}, new List<string>{"Insert"},
             new List<string>{"Opening", "Insert"}, new List<string>{"Ending", "Insert"}};
        SongTypes = list;
        RadioMode = false;
    }
    public async Task<Embed> ChangePlayers(string players, DBManager _db, AnilistService _as = null)
    {
        var playersTracked = await _db._rs.GetPlayersTracked();
        var playerArr = players.Split();
        foreach (string player in playerArr)
        {
            if (!playersTracked.ContainsKey(player))
                return await EmbedHandler.CreateErrorEmbed("Radio Service", $"Could not find {player} in the database. Radio is still set to {CurrPlayers}.");
        }
        CurrPlayers = players.Split().ToList();

        await UpdatePotentialSongs(_db, _as);
        return await EmbedHandler.CreateBasicEmbed("Radio Service", $"Radio player now set to {String.Join("  ", CurrPlayers)}", Color.Blue);
    }
    public string GetPlayers()
    {
        var sb = new StringBuilder();
        foreach(string player in CurrPlayers)
            sb.Append($"{player} ");
        return sb.ToString();
    }
    public async Task<Embed> SetType(int type, DBManager _db, AnilistService _as = null)
    {
        if (type > 0)
            CurrType = SongTypes[type - 1];
        else
            CurrType = SongTypes[type];

        await UpdatePotentialSongs(_db, _as);
        return await EmbedHandler.CreateBasicEmbed("Radio Service", $"Radio song type now set to {String.Join("  ", CurrType)}", Color.Blue);
    }
    public async Task<Embed> SetType(string type, DBManager _db, AnilistService _as = null)
    {
        if (SongTypes.Contains(type.Split().ToList()))
            CurrType = type.Split().ToList();
        await UpdatePotentialSongs(_db, _as);
        return await EmbedHandler.CreateBasicEmbed("Radio", $"Radio song type now set to {String.Join("  ", CurrType)}", Color.Blue);
    }
    public async Task<Embed> AddListStatus(string[] listArray, DBManager _db, AnilistService _as = null)
    {
        foreach (string listType in listArray)
        {
            listStatus.TryGetValue(listType.ToLower(), out int value);
            if (!ListNums.Contains(value))
                ListNums.Add(value);
        }
        StringBuilder toPrint = new StringBuilder();
        foreach (int thing in ListNums)
            toPrint.Append($"{listStatusReverse[thing]}\n");

        await UpdatePotentialSongs(_db, _as);
        return await EmbedHandler.CreateBasicEmbed("Radio", $"Radio player now set to play songs that are of the following list status \n{toPrint.ToString()}", Color.Blue);
    }
    public async Task<Embed> RemoveListStatus(string[] listArray, DBManager _db, AnilistService _as = null)
    {
        foreach (string listType in listArray)
        {
            listStatus.TryGetValue(listType.ToLower(), out int outVal);
            if (ListNums.Contains(outVal))
                ListNums.Remove(outVal);
        }
        StringBuilder toPrint = new StringBuilder();
        foreach (int thing in ListNums)
            toPrint.Append($"{listStatusReverse[thing]}\n");

        await UpdatePotentialSongs(_db, _as);
        return await EmbedHandler.CreateBasicEmbed("Radio", $"Radio player now set to play songs that are of the following list status \n{toPrint.ToString()}", Color.Blue);
    }
    public async Task<Embed> PrintRadio()
    {
        var sb = new StringBuilder();
        sb.Append("Current Players:\n");
        foreach (string player in CurrPlayers)
            sb.Append($"{player}\n");
        sb.Append("\nCurrent Types:\n");    
        foreach (string type in CurrType)
            sb.Append($"{type}\n");
        sb.Append("\nCurrent List Types:\n");     
        foreach (int num in ListNums)
            sb.Append($"{listStatusReverse[num]}\n");
        return await EmbedHandler.CreateBasicEmbed("Radio", sb.ToString(), Color.Blue);
    }    
    public async Task<Embed> ListTypes()
    {
        StringBuilder toPrint = new StringBuilder();
        var num = 1;
        foreach (List<string> s in SongTypes)
        {
            toPrint.Append($"Set the radio to play {String.Join("  ", s)} with !rct {num}.\n");
            num++;
        }
        return await EmbedHandler.CreateBasicEmbed("Radio", toPrint.ToString(), Color.Blue);
    }
    public async Task<Embed> PopulateQueue(List<SongTableObject> songs)
    {
        if (songs.Count == 0)
            return await EmbedHandler.CreateErrorEmbed("Radio", "No songs were found in this list.");

        Random rnd = new Random();
        for (int i = 0; i < songs.Count; i++)
        {
            int k = rnd.Next(0, i);
            var key = songs[k];
            songs[k] = songs[i];
            songs[i] = key;
        }
        foreach (SongTableObject song in songs)
            await Task.Run(() => Queue.Enqueue(song));
        return await EmbedHandler.CreateBasicEmbed("Radio", "The radio queue has been populated with songs use the !startradio command to begin listening.", Color.Blue);
    }
    public async Task DeQueue()
    {
        await Task.Run(() => Queue.TryDequeue(out var result));
    }
    public void DeQueueAll()
    {
        Queue.Clear();
    }
    public bool IsQueueEmpty()
    {
        if (Queue.Count > 0)
            return false;
        return true;
    }
    public async Task<SongTableObject> NextSong()
    {
        Queue.TryPeek(out var result);
        if (result != null)
        {
            await DeQueue();
            return result;
        }
        return null;
    }
    public List<SongTableObject> GetQueue()
    {
        return Queue.ToList();
    }
    public async Task UpdatePotentialSongs(DBManager _db, AnilistService _as = null)
    {
        List<SongTableObject> final = new List<SongTableObject>();
        List<SongTableObject> temp = new List<SongTableObject>();
        if (_as != null)
            temp.AddRange(await SongsFromAnimeListsAsync(_db, _as));
        foreach (string type in CurrType)
            final.AddRange(temp.Where(x => x.Type.ToLower().Contains(type.ToLower())));
        final = final.Distinct().ToList();
        SongSelection = final;
    }
    public async Task<List<SongTableObject>> SongsFromAnimeListsAsync(DBManager manager, AnilistService _as)
    {
        var songs = new List<SongTableObject>();
        var users = new List<DiscordUser>();
        var userAnilists = new List<UserAnilist>();
        using var db = new AMQDBContext();
        var playersTracked = await manager._rs.GetPlayersTracked();
        foreach (string name in CurrPlayers)
        {
            var list = await db.DiscordUsers
                        .AsNoTracking()
                        .Where(y => y.DatabaseName == playersTracked[name])
                        .ToListAsync();
            users.AddRange(list);
        }
        foreach (DiscordUser user in users)
        {
            if (user.AnilistName != null)
                userAnilists.Add(await _as.ReturnUserAnilistAsync(user.AnilistName, user.ID));
            if (user.MALName != null)
                songs.AddRange(await MALHandler.GetSongsFromMAL(user.MALName, ListNums));
        }
        songs.AddRange(await _as.ReturnSongsFromLists(userAnilists, ListNums));
        return songs;
    }
    public SongTableObject GetRandomSong()
    {
        if (SongSelection.Count == 0)
            return null;
        int r = rnd.Next(SongSelection.Count);
        return SongSelection[r];
    }
}
using System.Text;
using Discord;
using Discord.WebSocket;
using PartyBot.Handlers;
using PartyBot.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using PartyBot.Database;
using Victoria;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.IO;
using PartyBot.DataStructs;

public class Radio
{
    public bool RadioMode;
    public string CurrType;
    public string CurrPlayers;
    private readonly List<string> SongTypes;
    public readonly SocketGuild Guild;
    public readonly IMessageChannel Channel;
    public List<int> ListNums = new List<int>();
    public Dictionary<string, int> listStatus = new Dictionary<string, int>();
    public Dictionary<int, string> listStatusReverse = new Dictionary<int, string>();
    private List<SongTableObject> SongSelection = new List<SongTableObject>();
    private Queue<SongTableObject> Queue = new Queue<SongTableObject>();
    public Radio(IMessageChannel c, SocketGuild g)
    {
        Guild = g;
        Channel = c;
        CurrType = "Opening Ending";
        CurrPlayers = "any";
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
        SongTypes = new List<string> {"Opening",
            "Opening Ending", "Ending", "Opening Ending Insert", "Insert", "Opening Insert", "Ending Insert"};
        RadioMode = false;
    }

    public async Task<Embed> ChangePlayer(string players, DBManager _db, AnilistService _as = null)
    {
        var playersTracked = await _db._rs.GetPlayersTracked();
        var playerArr = players.Split();
        foreach (string player in playerArr)
        {
            if (!playersTracked.ContainsKey(player))
                return await EmbedHandler.CreateErrorEmbed("Radio Service", $"Could not find {player} in the database. Radio is still set to {CurrPlayers}.");
        }
        CurrPlayers = players;

        await UpdatePotentialSongs(_db, _as);
        return await EmbedHandler.CreateBasicEmbed("Radio Service", $"Radio player now set to {CurrPlayers}", Color.Blue);
    }
    public async Task<Embed> SetType(int type, DBManager _db, AnilistService _as = null)
    {
        if (type > 0)
            CurrType = SongTypes[type - 1];
        else
            CurrType = SongTypes[type];

        await UpdatePotentialSongs(_db, _as);
        return await EmbedHandler.CreateBasicEmbed("Radio Service", $"Radio song type now set to {CurrType}", Color.Blue);
    }
    public async Task<Embed> SetType(string type, DBManager _db, AnilistService _as = null)
    {
        if (SongTypes.Contains(type))
            CurrType = type;
        await UpdatePotentialSongs(_db, _as);
        return await EmbedHandler.CreateBasicEmbed("Radio", $"Radio song type now set to {CurrType}", Color.Blue);
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
        string toPrint = $"Current Players:\n{CurrPlayers}\nCurrent Types:\n{CurrType}\n";
        return await EmbedHandler.CreateBasicEmbed("Radio", toPrint, Color.Blue);
    }
    public async Task<Embed> ListTypes()
    {
        StringBuilder toPrint = new StringBuilder();
        foreach (string s in SongTypes)
            toPrint.Append($"{s}\n");

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

    public async Task<List<string>> PrintQueue()
    {
        var sb = new StringBuilder();
        var array = await Task.Run(() => Queue.ToArray());
        var stringArray = new List<string>();
        foreach (SongTableObject song in array)
            stringArray.Add(SongTableObject.PrintSong(song));

        return stringArray;
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

    public async Task UpdatePotentialSongs(DBManager _db, AnilistService _as = null)
    {
        string[] types = CurrType.Split(" ");
        List<SongTableObject> potentialSongs = new List<SongTableObject>();
        List<SongTableObject> final = new List<SongTableObject>();
        if (_as != null)
            final.AddRange(await SongsFromAnimeListsAsync(_as));
        final = final.Where(x => types.Contains(x.Type)).ToList();
        //loop through each desired type
        foreach (string type in types)
        {
            if (CurrPlayers.Equals("any"))
            {
                var Query = await DBSearchService.ReturnAllSongObjectsByType(type);
                final.AddRange(Query);
                continue;
            }
            //loop through each desired list status
            foreach (int num in ListNums)
            {
                //loop through each player set in the radio
                foreach (string player in CurrPlayers.Split())
                {
                    var playersTracked = await _db._rs.GetPlayersTracked();
                    var Query = await DBSearchService.ReturnAllPlayerObjects(playersTracked[player], type, num, "");
                    foreach (PlayerTableObject pto in Query)
                        potentialSongs.Add(await DBSearchService.UseSongKey(SongTableObject.MakeSongTableKey(pto)));
                    final.AddRange(potentialSongs);
                }
            }
        }
        final = final.Distinct().ToList();
        SongSelection = final;
    }

    public async Task<List<SongTableObject>> SongsFromAnimeListsAsync(AnilistService _as)
    {
        var songs = new List<SongTableObject>();
        var users = new List<DiscordUser>();
        var userAnilists = new List<UserAnilist>();
        string[] types = CurrType.Split(" ");
        using var db = new AMQDBContext();
        var playersSplit = CurrPlayers.Split();
        foreach (string name in playersSplit)
        {
            var list = await db.DiscordUsers
                        .AsNoTracking()
                        .ToListAsync();
            list = list.Where(y => y.DatabaseName == name).ToList();
            users.AddRange(list);
        }
        foreach (DiscordUser user in users)
            userAnilists.Add(await _as.ReturnUserAnilistAsync(user.AnilistName));
        return await _as.ReturnSongsFromLists(userAnilists, ListNums);
    }

    public SongTableObject GetRandomSong()
    {
        if (SongSelection.Count == 0)
            return null;
        Random rnd = new Random();
        int r = rnd.Next(SongSelection.Count);
        return SongSelection[r];
    }
}
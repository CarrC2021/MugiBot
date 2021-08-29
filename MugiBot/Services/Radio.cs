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

public class Radio
{
    public bool RadioMode;
    public string CurrType;
    public string CurrPlayers;
    private readonly List<string> SongTypes;
    public readonly SocketGuild Guild;
    public readonly IMessageChannel Channel;
    public List<int> listNums = new List<int>();
    public Dictionary<string, int> listStatus = new Dictionary<string, int>();
    public Dictionary<int, string> listStatusReverse = new Dictionary<int, string>();
    private List<SongTableObject> SongSelection = new List<SongTableObject>();
    public Radio(IMessageChannel c, SocketGuild g)
    {
        Guild = g;
        Channel = c;
        CurrType = "Opening Ending";
        CurrPlayers = "any";
        listNums.AddRange(new int[] { 1, 2, 3, 4, 5 });
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

    public async Task<Embed> ChangePlayer(string players, DBManager _db)
    {
        var playersTracked = await _db._rs.GetPlayersTracked();
        var playerArr = players.Split();
        foreach (string player in playerArr)
        {
            if (!playersTracked.ContainsKey(player))
                return await EmbedHandler.CreateErrorEmbed("Radio Service", $"Could not find {player} in the database. Radio is still set to {CurrPlayers}.");
        }
        CurrPlayers = players;

        await UpdatePotentialSongs(_db);
        return await EmbedHandler.CreateBasicEmbed("Radio Service", $"Radio player now set to {CurrPlayers}", Color.Blue);
    }
    public async Task<Embed> SetType(int type, DBManager _db)
    {
        if (type > 0)
            CurrType = SongTypes[type - 1];
        else
            CurrType = SongTypes[type];

        await UpdatePotentialSongs(_db);
        return await EmbedHandler.CreateBasicEmbed("Radio Service", $"Radio song type now set to {CurrType}", Color.Blue);
    }
    public async Task<Embed> SetType(string type, DBManager _db)
    {
        if (SongTypes.Contains(type))
            CurrType = type;
        await UpdatePotentialSongs(_db);
        return await EmbedHandler.CreateBasicEmbed("Radio", $"Radio song type now set to {CurrType}", Color.Blue);
    }
    public async Task<Embed> AddListStatus(string[] listArray, DBManager _db)
    {
        foreach (string listType in listArray)
        {
            listStatus.TryGetValue(listType.ToLower(), out int value);
            if (!listNums.Contains(value))
                listNums.Add(value);
        }
        StringBuilder toPrint = new StringBuilder();
        foreach (int thing in listNums)
            toPrint.Append($"{listStatusReverse[thing]}\n");

        await UpdatePotentialSongs(_db);
        return await EmbedHandler.CreateBasicEmbed("Radio", $"Radio player now set to play songs that are of the following list status \n{toPrint.ToString()}", Color.Blue);
    }
    public async Task<Embed> RemoveListStatus(string[] listArray, DBManager _db)
    {
        foreach (string listType in listArray)
        {
            listStatus.TryGetValue(listType.ToLower(), out int outVal);
            if (listNums.Contains(outVal))
                listNums.Remove(outVal);
        }
        StringBuilder toPrint = new StringBuilder();
        foreach (int thing in listNums)
            toPrint.Append($"{listStatusReverse[thing]}\n");

        await UpdatePotentialSongs(_db);
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

    public async Task UpdatePotentialSongs(DBManager _db)
    {
        string[] types = CurrType.Split(" ");
        List<SongTableObject> potentialSongs = new List<SongTableObject>();
        List<SongTableObject> final = new List<SongTableObject>();
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
            foreach (int num in listNums)
            {
                //loop through each player set in the radio
                foreach (string player in CurrPlayers.Split())
                {
                    var playersTracked = await _db._rs.GetPlayersTracked();
                    var Query = await DBSearchService.ReturnAllPlayerObjects(playersTracked[player], type, num, "");
                    foreach (PlayerTableObject pto in Query)
                    {
                        potentialSongs.Add(await DBSearchService.UseSongKey(SongTableObject.MakeSongTableKey(pto)));
                    }
                    final.AddRange(potentialSongs);
                }
            }
        }
        SongSelection = final;
    }

    public SongTableObject GetRandomSong()
    {
        Random rnd = new Random();
        int r = rnd.Next(SongSelection.Count);
        return SongSelection[r];
    }
}
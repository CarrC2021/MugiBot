using Discord;
using Discord.WebSocket;
using PartyBot.Handlers;
using PartyBot.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        RadioMode = true;
    }

    public async Task<Embed> ChangePlayer(string players, PlayersRulesService _prs)
    {
        var playersTracked = await _prs.GetPlayersTracked();
        var playerArr = players.Split();
        foreach (string player in playerArr)
        {
            if (!playersTracked.ContainsKey(player))
                return await EmbedHandler.CreateBasicEmbed("Radio Service", $"Radio player now set to {CurrPlayers}", Color.Blue);
        }
        CurrPlayers = players;
        return await EmbedHandler.CreateBasicEmbed("Radio Service", $"Radio player now set to {CurrPlayers}", Color.Blue);
    }
    public async Task<Embed> SetType(int type)
    {
        if (type > 0)
            CurrType = SongTypes[type - 1];
        else
            CurrType = SongTypes[type];
        return await EmbedHandler.CreateBasicEmbed("Radio Service", $"Radio song type now set to {CurrType}", Color.Blue);
    }
    public async Task<Embed> SetType(string type)
    {
        if (SongTypes.Contains(type))
            CurrType = type;
        return await EmbedHandler.CreateBasicEmbed("Radio", $"Radio song type now set to {CurrType}", Color.Blue);
    }
    public async Task<Embed> AddListStatus(string[] listArray)
    {
        foreach (string listType in listArray)
        {
            listStatus.TryGetValue(listType.ToLower(), out int value);
            if (!listNums.Contains(value))
                listNums.Add(value);
        }
        string toPrint = "";
        foreach (int thing in listNums)
            toPrint += $"{listStatusReverse[thing]}\n";

        return await EmbedHandler.CreateBasicEmbed("Radio", $"Radio player now set to play songs that are of the following list status \n{toPrint}", Color.Blue);
    }
    public async Task<Embed> RemoveListStatus(string[] listArray)
    {
        foreach (string listType in listArray)
        {
            listStatus.TryGetValue(listType, out int outVal);
            if (listNums.Contains(outVal))
                listNums.Remove(outVal);
        }
        string toPrint = "";
        foreach (int thing in listNums)
            toPrint += listStatusReverse[thing] + "\n";

        return await EmbedHandler.CreateBasicEmbed("Radio", $"Radio player now set to play songs that are of the following list status \n{toPrint}", Color.Blue);
    }
    public async Task<Embed> PrintRadio()
    {
        string toPrint = $"{CurrPlayers}\n{CurrType}";
        return await EmbedHandler.CreateBasicEmbed("Radio", toPrint, Color.Blue);
    }
    public async Task<Embed> ListTypes()
    {
        string toPrint = "";
        foreach (string s in SongTypes)
            toPrint += $"{s}\n";

        return await EmbedHandler.CreateBasicEmbed("Radio", toPrint, Color.Blue);
    }


}
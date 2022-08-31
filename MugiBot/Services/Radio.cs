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
using System.IO;

public class Radio
{
    public bool Autoplay;
    public List<string> CurrType;
    public List<string> CurrPlayers;
    private readonly List<List<string>> SongTypes;
    public readonly SocketGuild Guild;
    public readonly IMessageChannel Channel;
    public List<int> ListNums = new List<int>();
    public Dictionary<string, int> listStatus = new Dictionary<string, int>();
    public Dictionary<int, string> listStatusReverse = new Dictionary<int, string>();
    private List<SongTableObject> SongSelection = new List<SongTableObject>();
    public List<SongTableObject> Queue = new List<SongTableObject>();
    private readonly Random rnd;
    public Radio(IMessageChannel c, SocketGuild g)
    {
        rnd = new Random();
        Guild = g;
        Channel = c;
        CurrType = new List<string>{"Opening", "Ending"};
        CurrPlayers = new List<string>{"any"};
        ListNums.AddRange(new int[] { 1, 2, 3, 4, 5 });

        // Unfortunately we need to use these integers to specify list status 
        // since anilist and mal are not consistent in naming convention.
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
        Autoplay = false;
    }
    private string CurrentPlayersString()
    {
        return String.Join("  ", CurrPlayers);
    }
    private string CurrentTypeString()
    {
        return String.Join("  ", CurrType);
    }
    /// <summary>
    /// This is an asynchronous function to set the current players to what the user provides. If one of the names
    /// provided in the string has not had their list downloaded then there will be an error message.
    /// <summary>
    /// <param name="players"> A string representing the name of players who have their lists downloaded by the project,
    /// where each player is separated by a space character and whose list will be used by the bot to play music off.
    /// </param>
    /// <returns> a <see cref="Discord.Embed"/> once the asynchronous task is completed. </returns>
    public async Task<Embed> ChangePlayers(string players, AnilistService _as = null)
    {
        var playerArr = players.Split();
        var fileList = Directory.GetFiles(Path.Combine(GlobalData.Config.RootFolderPath, "AniLists")).Select(f => Path.GetFileName(f)).ToList();
        fileList.AddRange((Directory.GetFiles(Path.Combine(GlobalData.Config.RootFolderPath, "MALUserLists")).Select(f => Path.GetFileName(f))));
        foreach (string player in playerArr)
        {
            if (!fileList.Contains($"{player}.json"))
                return await EmbedHandler.CreateErrorEmbed("Radio Service", $"Could not find {player}'s list in the database. To see the lists in the bot's database use !printlists. "+
                $"Radio is still set to {CurrentPlayersString()}.");
        }
        CurrPlayers = players.Split().ToList();

        await UpdatePotentialSongs(_as);
        return await EmbedHandler.CreateBasicEmbed("Radio Service", $"Radio player now set to {CurrentPlayersString()}", Color.Blue);
    }
    public string GetPlayers()
    {
        var sb = new StringBuilder();
        foreach(string player in CurrPlayers)
            sb.Append($"{player} ");
        return sb.ToString();
    }
    /// <summary>
    /// This is an asynchronous function to set the type of songs to be played to what the user provides.
    /// <summary>
    /// <param name="type"> An int representing the type of songs that the user wants played.
    /// </param>
    /// <returns> a <see cref="Discord.Embed"/> once the asynchronous task is completed. </returns>
    public async Task<Embed> SetType(int type, AnilistService _as = null)
    {
        if (type > 0)
            CurrType = SongTypes[type - 1];
        else
            CurrType = SongTypes[type];

        await UpdatePotentialSongs(_as);
        return await EmbedHandler.CreateBasicEmbed("Radio Service", $"Radio song type now set to {CurrentTypeString()}", Color.Blue);
    }
    /// <summary>
    /// This is an asynchronous function to set the type of songs to be played to what the user provides.
    /// <summary>
    /// <param name="type"> A string representing the type of songs that the user wants played.
    /// </param>
    /// <returns> a <see cref="Discord.Embed"/> once the asynchronous task is completed. </returns>
    public async Task<Embed> SetType(string type, AnilistService _as = null)
    {
        if (SongTypes.Contains(type.Split().ToList()))
            CurrType = type.Split().ToList();
        await UpdatePotentialSongs(_as);
        return await EmbedHandler.CreateBasicEmbed("Radio", $"Radio song type now set to {CurrentTypeString()}", Color.Blue);
    }
    /// <summary>
    /// This is an asynchronous function to add the list status that the user specifies and then update the song selection.
    /// <summary>
    /// <param name="listArray"> A list of strings representing list status of shows to add to the selection.
    /// </param>
    /// <returns> a <see cref="Discord.Embed"/> once the asynchronous task is completed. </returns>
    public async Task<Embed> AddListStatus(string[] listArray, AnilistService _as = null)
    {
        foreach (string listType in listArray)
        {
            // Add the list status if it is not already in the list
            listStatus.TryGetValue(listType.ToLower(), out int value);
            if (!ListNums.Contains(value))
                ListNums.Add(value);
        }
        StringBuilder toPrint = new StringBuilder();
        foreach (int num in ListNums)
            toPrint.Append($"{listStatusReverse[num]}\n");

        await UpdatePotentialSongs(_as);
        return await EmbedHandler.CreateBasicEmbed("Radio", $"Radio player now set to play songs that are of the following list status \n{toPrint.ToString()}", Color.Blue);
    }
    /// <summary>
    /// This is an asynchronous function to remove the list status that the user specifies and then update the song selection.
    /// <summary>
    /// <returns> a <see cref="Discord.Embed"/> once the asynchronous task is completed. </returns>
    public async Task<Embed> RemoveListStatus(string[] listArray, AnilistService _as = null)
    {
        foreach (string listType in listArray)
        {
            // If the current listStatus contains of the status the user specifies then remove it
            listStatus.TryGetValue(listType.ToLower(), out int outVal);
            if (ListNums.Contains(outVal))
                ListNums.Remove(outVal);
        }
        var toPrint = new StringBuilder();
        foreach (int thing in ListNums)
            toPrint.Append($"{listStatusReverse[thing]}\n");

        await UpdatePotentialSongs(_as);
        return await EmbedHandler.CreateBasicEmbed("Radio", $"Radio player now set to play songs that are of the following list status \n{toPrint.ToString()}", Color.Blue);
    }
    /// <summary>
    /// This is an asynchronous function to return an embed telling the user the current state of the Radio object.
    /// <summary>
    /// <returns> a <see cref="Discord.Embed"/> once the asynchronous task is completed. </returns>
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
    /// <summary>
    /// This is an asynchronous function to return an embed telling the user which integer 1-6 correspond to which list types.
    /// <summary>
    /// <returns> a <see cref="Discord.Embed"/> once the asynchronous task is completed. </returns>
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
    /// <summary>
    /// This is an asynchronous function that populates the queue using the given list of <see cref="PartyBot.DataStructs.SongTableObject"/>.
    /// <summary>
    /// <returns> a <see cref="Discord.Embed"/> once the asynchronous task is completed. </returns>
    public async Task<Embed> PopulateQueue(List<SongTableObject> songs)
    {
        if (songs.Count == 0)
            return await EmbedHandler.CreateErrorEmbed("Radio", "No songs were found in this list.");
        // randomize the order of the songs
        songs = songs.OrderBy (x => rnd.Next()).ToList();
        foreach (SongTableObject song in songs)
            Queue.Add(song);
        return await EmbedHandler.CreateBasicEmbed("Radio", "The radio queue has been populated with songs use the !startradio command to begin listening.", Color.Blue);
    }
    public async Task DeQueue()
    {
        await Task.Run(() => Queue.RemoveAt(0));
    }
    public bool IsQueueEmpty()
    {
        if (Queue.Count > 0)
            return false;
        return true;
    }
    
    /// <summary>
    /// This is an asynchronous function that returns a song at the top of the queue in the radio's queue.
    /// <summary>
    /// <returns> a <see cref="PartyBot.DataStructs.SongTableObject"/> once the asynchronous task is completed. </returns>
    public async Task<SongTableObject> NextSong()
    {
        try
        {
            var toReturn = Queue[0];
            await DeQueue();
            return toReturn;
        }
        catch (Exception ex)
        {
            await LoggingService.LogCriticalAsync(ex.Source, ex.Message);
            return null;
        }
    }
    /// <summary>
    /// This is an asynchronous function that shuffles the radio's queue.
    /// <summary>
    /// <returns> a <see cref="Discord.Embed"/> once the asynchronous task is completed. </returns>
    public async Task<Embed> ShuffleQueue()
    {
        Queue = Queue.OrderBy (x => rnd.Next()).ToList();
        return await EmbedHandler.CreateBasicEmbed("Radio Queue", "The radio queue has been shuffled, use !queue to confirm.", Color.Blue);
    }
    public List<SongTableObject> GetQueue()
    {
        return Queue;
    }
    public async Task UpdatePotentialSongs(AnilistService _as = null)
    {
        List<SongTableObject> final = new List<SongTableObject>();
        List<SongTableObject> temp = new List<SongTableObject>();
        if (_as != null)
            temp.AddRange(await SongsFromAnimeListsAsync(_as));
        foreach (string type in CurrType)
            final.AddRange(temp.Where(x => x.Type.ToLower().Contains(type.ToLower())));
        final = final.Distinct().ToList();
        SongSelection = final;
    }
    /// <summary>
    /// This is an asynchronous function that retrieves the list of potential songs to be played by the radio,
    /// from the CurrPlayers anime lists. This is either retrieved from data in the form of
    /// a <see cref="PartyBot.DataStructs.UserAnilist"/> or <see cref="PartyBot.DataStructs.MalUserList"/>.
    /// <summary>
    /// <returns> a list of <see cref="SongTableObject>"/> objects once the asynchronous task is completed. </returns>
    public async Task<List<SongTableObject>> SongsFromAnimeListsAsync(AnilistService _as)
    {
        var songs = new List<SongTableObject>();
        var userAnilists = new List<UserAnilist>();
        foreach (string name in CurrPlayers)
        {
            // retrieve data from any matching UserAnilist
            if (File.Exists(Path.Combine(GlobalData.Config.RootFolderPath, "AniLists", $"{name}.json")))
                userAnilists.Add(await _as.ReturnUserAnilistAsync(name, 0));
            // retrieve data from any matching MalUserList
            if (File.Exists(Path.Combine(GlobalData.Config.RootFolderPath, "MALUserLists", $"{name}.json")))
                songs.AddRange(await MALHandler.GetSongsFromMAL(name, ListNums));
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
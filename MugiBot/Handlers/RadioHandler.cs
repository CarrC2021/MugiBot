using Discord;
using Discord.WebSocket;
using PartyBot.Database;
using PartyBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace PartyBot.Handlers
{
    public class RadioHandler
    {
        public static async Task<Embed> TurnOff(Radio radio)
        {
            if (radio == null)
                return await EmbedHandler.CreateBasicEmbed("Radio Service", $"There is no radio in this server", Color.Blue);

            radio.RadioMode = !radio.RadioMode;
            return await EmbedHandler.CreateBasicEmbed("Radio Service", $"Radio has been turned off", Color.Blue);
        }
        public static Radio FindRadio(List<Radio> radios, SocketGuild guild)
        {
            IEnumerable<Radio> temp = radios.Where(d => d.Guild == guild);
            if (temp.FirstOrDefault() == null)
                return null;
            return temp.FirstOrDefault();
        }
        public static Radio FindOrCreateRadio(List<Radio> radios, ISocketMessageChannel channel, SocketGuild guild)
        {
            IEnumerable<Radio> temp = radios.Where(d => d.Guild == guild);
            if (temp.FirstOrDefault() == null)
            {
                Radio toAdd = new Radio(channel, guild);
                radios.Add(toAdd);
                return toAdd;
            }
            return temp.FirstOrDefault();
        }
        public static async Task<SongTableObject> GetRandomRadioSong(Radio radio, PlayersRulesService _prs)
        {
            Random rnd = new Random();
            string[] types = radio.CurrType.Split(" ");
            List<SongTableObject> potentialSongs = new List<SongTableObject>();
            List<SongTableObject> final = new List<SongTableObject>();
            List<int> listNums = radio.listNums;
            //loop through each desired type
            foreach (string type in types)
            {
                if (radio.CurrPlayers.Equals("any"))
                {
                    var Query = await DBSearchService.ReturnAllSongObjectsByType(type);
                    potentialSongs = Query
                        .ToList();
                    final.AddRange(potentialSongs);
                    continue;
                }
                //loop through each desired list status
                foreach (int num in listNums)
                {
                    //loop through each player set in the radio
                    foreach (string player in radio.CurrPlayers.Split())
                    {
                        var playersTracked = await _prs.GetPlayersTracked();
                        var Query = await DBSearchService.ReturnAllPlayerObjects(playersTracked[player], type, num, "");
                        foreach (PlayerTableObject pto in Query)
                        {
                            potentialSongs.Add(await DBSearchService.UseSongKey(SongTableObject.MakeSongTableKey(pto)));
                        }
                        final.AddRange(potentialSongs);
                    }
                }
            }
            int r = rnd.Next(final.Count);

            return final[r];
        }
    }
}


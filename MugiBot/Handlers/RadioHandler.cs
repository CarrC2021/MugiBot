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

            radio.RadioMode = false;
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
        
    }
}


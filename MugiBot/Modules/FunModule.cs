using Discord;
using Discord.Commands;
using PartyBot.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyBot.Modules
{
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        [Command("CatJam")]
        [Summary("Will catJam in the chat.")]
        public async Task CatJam()
            => await Context.Channel.SendMessageAsync("https://tenor.com/view/cat-jam-gif-18110512");

        [Command("RateWaifu")]
        [Summary("Type in the name of a character and they will be rated.")]
        public async Task RateWaifu([Remainder] string name)
        {
            var vals = new List<int>();

            foreach (char c in name)
            {
                vals.Add(Convert.ToInt32(c));
            }
            int val = vals.Cast<int>().Sum() % 11;

            //gotta make sure people know the truth
            if (name.ToLower().Equals("yotsuba"))
            {
                val = 2;
            }
            await Context.Channel.SendMessageAsync(embed: await EmbedHandler.CreateBasicEmbed("Waifu Rating", name + " is a " + val, Color.Blue));
        }

        [Command("DogJam")]
        [Summary("Will dogJam in the chat.")]
        public async Task DogJam()
            => await Context.Channel.SendMessageAsync("https://tenor.com/view/dance-uporot-brazil-gif-13264739");
    }
}
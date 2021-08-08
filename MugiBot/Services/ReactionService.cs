using Discord;
using Discord.WebSocket;
using PartyBot.Database;
using PartyBot.Handlers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyBot.Services
{

    public class ReactionService
    {
        public List<Emoji> OneToTen = new List<Emoji>();
        public List<string> Unicodes = new List<string>();

        public List<Emoji> YesOrNo = new List<Emoji>();
        Emote pepega = Emote.Parse("<:pepega:858850328320933888>");

        public ReactionService()
        {
            OneToTen.Add(new Emoji("1️⃣"));
            OneToTen.Add(new Emoji("2️⃣"));
            OneToTen.Add(new Emoji("3️⃣"));
            OneToTen.Add(new Emoji("4️⃣"));
            OneToTen.Add(new Emoji("5️⃣"));
            OneToTen.Add(new Emoji("6️⃣"));
            OneToTen.Add(new Emoji("7️⃣"));
            OneToTen.Add(new Emoji("8️⃣"));
            OneToTen.Add(new Emoji("9️⃣"));
            OneToTen.Add(new Emoji("🔟"));
            Unicodes.Add("1️⃣");
            Unicodes.Add("2️⃣");
            Unicodes.Add("3️⃣");
            Unicodes.Add("4️⃣");
            Unicodes.Add("5️⃣");
            Unicodes.Add("6️⃣");
            Unicodes.Add("7️⃣");
            Unicodes.Add("8️⃣");
            Unicodes.Add("9️⃣");
            Unicodes.Add("🔟");
            YesOrNo.Add(new Emoji(":thumbsup:"));
            YesOrNo.Add(new Emoji(":thumbsdown:"));
        }

        public async Task AddOneToTenReaction(SocketMessage message)
        {
            var body = message.Embeds.FirstOrDefault().Description;
            string[] listofstuff = body.Split("\n");
            for (int i = 0; i < listofstuff.Length; i++)
            {
                await message.AddReactionAsync(OneToTen[i]);
            }
        }
        public async Task AddOneToTenReaction(IUserMessage message)
        {
            var body = message.Embeds.FirstOrDefault().Description;
            string[] listofstuff = body.Split("\n");
            for (int i = 0; i < listofstuff.Length; i++)
                await message.AddReactionAsync(OneToTen[i]);
        }

        public async Task AddYesOrNoReaction(SocketMessage message)
        {
            foreach (Emoji emoji in OneToTen)
                await message.AddReactionAsync(emoji);
        }

        public async Task EmbedReceived(SocketMessage message)
        {
            string title = message.Embeds.First().Title;
            if (title.Equals("Data, Search") || title.Equals("Data, Recommendation"))
                await Task.Run(async () => await AddOneToTenReaction(message));
        }

        private async Task PepegaReceived(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction, DBManager _db, LavaLinkAudio _audioservice)
        {
            var tempChannel = (SocketTextChannel)channel;
            var user = await reaction.Channel.GetUserAsync(reaction.UserId) as SocketGuildUser;
            List<string> songCollection = new List<string>();
            var trimmedBody = ReturnTrimmedMessage(message);
            foreach (string line in trimmedBody)
                songCollection.Add(line);


            //var outKey = songCollection.First();
            //var table = await DBSearchService.UseSongKey(outKey);
            //await tempChannel.SendMessageAsync(embed: await EmbedHandler.CreateBasicEmbed("Music", $"Now Playing {table.Show} {table.Type} {table.SongName} by {table.Artist}", Color.Blue));
            //songCollection.Remove(songCollection.First());
            //foreach (string songKey in songCollection)
                //await _audioservice.QueueCatboxFromDB(songKey, user, tempChannel.Guild);
        }

        public async Task ReactionReceieved(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction, DBManager _db, LavaLinkAudio _audioservice)
        {
            if (channel.Name.Equals("bot-commands") || channel.Name.Equals("file-uploads"))
            {
                if (Unicodes.Contains(reaction.Emote.Name))
                {
                    var result = await cachedMessage.GetOrDownloadAsync();
                    await OneToTenReceived(result, channel, reaction, _db, _audioservice);
                }
                if (reaction.Emote.Name == pepega.Name)
                {
                    var result = await cachedMessage.GetOrDownloadAsync();
                    await PepegaReceived(result, channel, reaction, _db, _audioservice);
                }
            }
        }

        public async Task OneToTenReceived(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction, DBManager _db, LavaLinkAudio _audioservice)
        {
            //var tempChannel = (SocketTextChannel)channel;
            //var user = await reaction.Channel.GetUserAsync(reaction.UserId) as SocketGuildUser;
            //var trimmedBody = ReturnTrimmedMessage(message);
            //await tempChannel.SendMessageAsync(embed: await _audioservice.QueueCatboxFromDB(
                //trimmedBody[Unicodes.IndexOf(reaction.Emote.Name)], user, tempChannel.Guild));
        }

        public string[] ReturnTrimmedMessage(IUserMessage message)
        {
            var body = message.Embeds.FirstOrDefault().Description;
            string[] description = body.Split("\n");
            var trimmedBody = description
                    .Where(f => !f.Contains("Times Played: "))
                    .Where(j => !j.Contains("Success Rate: "))
                    .Where(j => !j.Contains("https://"))
                    .Where(k => !k.Contains("Total: "))
                    .ToArray();
            return trimmedBody;
        }
    }
}



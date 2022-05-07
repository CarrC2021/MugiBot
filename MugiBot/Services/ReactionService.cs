using Discord;
using Discord.WebSocket;
using PartyBot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PartyBot.Handlers;

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

        // This adds a thumbs up and thumbs down reaction. 
        public async Task AddYesOrNoReaction(SocketMessage message)
        {
            foreach (Emoji emoji in YesOrNo)
                await message.AddReactionAsync(emoji);
        }

        public async Task EmbedReceived(SocketMessage message)
        {
            string title = message.Embeds.First().Title;
            if (title.Equals("Data, Search") || title.Equals("Data, Recommendation"))
                await Task.Run(async () => await AddYesOrNoReaction(message));
        }

        // If pepega is added as a reaction to an embed that contains song keys it will queue
        // all of the songs in that embed.
        private async Task PepegaReceived(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction, DBManager _db, LavaLinkAudio _audioservice)
        {
            var tempChannel = (SocketTextChannel)channel;
            var user = await reaction.Channel.GetUserAsync(reaction.UserId) as SocketGuildUser;
            var trimmedBody = ReturnTrimmedMessage(message);

            // Unfortunately have to do this hacky method to get an embed to send for the first song
            await channel.SendMessageAsync(embed: await _audioservice.QueueCatboxFromDB(trimmedBody.First(), user, tempChannel.Guild));
            trimmedBody.RemoveAt(0);

            // Now loop through the rest of the songs and queue them
            foreach (string songKey in trimmedBody)
                await _audioservice.QueueCatboxFromDB(songKey, user, tempChannel.Guild);
        }

        // Anytime a reaction is received by the bot this function will be called.
        public async Task ReactionReceieved(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction, DataService _dataService, LavaLinkAudio _audioservice)
        {
            try
            {
                IUserMessage result = await cachedMessage.DownloadAsync();
                if(result.Author.Id == GlobalData.Config.Id && (Unicodes.Contains(reaction.Emote.Name)))
                    await OneToTenReceived(result, channel, reaction, _dataService.DBManager, _audioservice);

                if(result.Author.Id == GlobalData.Config.Id && reaction.Emote.Name == pepega.Name)
                    await PepegaReceived(result, channel, reaction, _dataService.DBManager, _audioservice);

            }
            catch(Exception ex)
            {
                await EmbedHandler.CreateErrorEmbed("Reaction handler", $"Error Message: {ex.Message}\n Stack Trace: {ex.StackTrace}");
            }
            
        }

        // If the bot receives a one to ten reaction on an embed with song keys on it,
        // then this function will queue the song that corresponds to the number.
        public async Task OneToTenReceived(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction, DBManager _db, LavaLinkAudio _audioservice)
        {
            var tempChannel = (SocketTextChannel)channel;
            var user = await reaction.Channel.GetUserAsync(reaction.UserId) as SocketGuildUser;
            Console.WriteLine(user.Nickname + user.Id);
            var trimmedBody = ReturnTrimmedMessage(message);
            try
            {
                Console.WriteLine("Just before QueueCatbox function is called.");
                await tempChannel.SendMessageAsync(embed: await _audioservice.QueueCatboxFromDB(
                    trimmedBody[Unicodes.IndexOf(reaction.Emote.Name)], user, tempChannel.Guild));
            }
            catch (Exception ex)
            {
                await tempChannel.SendMessageAsync(embed: await EmbedHandler.CreateErrorEmbed("Reaction Service", $"Something went wrong here. \n {ex.Message}"));
            }
        }

        // Removes all lines from the body of the message except for the lines containing the song keys
        public List<string> ReturnTrimmedMessage(IUserMessage message)
        {
            var body = message.Embeds.FirstOrDefault().Description;
            string[] description = body.Split("\n");
            List<string> trimmed = description.SkipWhile(x => !x.Contains("All keys:")).ToList();
            trimmed.Remove(trimmed[0]);
            return trimmed;
        }
    }
}



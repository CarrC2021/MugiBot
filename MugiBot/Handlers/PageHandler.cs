using Discord;
using Discord.WebSocket;
using PartyBot.Database;
using PartyBot.Handlers;
using System.Threading.Tasks;

public class PageHandler
{
    public static async Task<Embed> TurnPage(IUserMessage message, ISocketMessageChannel channel, SocketReaction reaction, DBManager _db)
    {
        return await EmbedHandler.CreateBasicEmbed("", "", Color.Green);
    }
}

using Discord;
using Discord.Webhook;
using PartyBot.Handlers;

namespace PartyBot.DataStructs
{
    public class HelpEmbeds
    {
        public readonly string AudioHelpString =
        "In order to use almost any audio command you need to be in a voice channel. "
        + "So make sure you have joined one.\n The next step is to use the !join command to get the bot to join the voice channel."
        + "\nOnce the bot is in the voice channel you can play youtube videos, mp3 links, etc with the !play command."
        + " Note, if you do not type a direct link after the !play, it will search youtube and play some video."
        + "\nIf you do not know the link to a catbox song, don't worry you can use any of the search commands and they will"
        + "give you something called a key. Just copy the key to the song you want and then use !playkey to play them."
        + "\n If you don't like the current song use !skip to skip that song. \nTo see a list of what is currently playing use !list."
        + "\nTo pause the playback use !pause, to resume the playback use !resume. \nTo get the bot to stop playing and clear the queue, use"
        + "!stop. \nTo get the bot to leave the voice channel use !leave.";

        public readonly string DatabaseTrackingHelpString = 
        "If you want to track a player's stats in the database use the command !addplayer."
        + " Here is an example of how to use it, !addplayer dingster A_Real_Dingus."
        + " What that does is track the amq user name dingster in the database as A_Real_Dingus."
        + " This is useful for when you have smurfs and want multiple amq user names under the same name in the database."
        + " To see the players the bot is tracking use the !listplayers command."
        + "\n Now in order to update the database you first have to upload jsons exported using Joseph98's song list ui script."
        + " Make sure the channel you upload to is named file-uploads otherwise the bot will ignore it."
        + " To confirm the bot has downloaded the file you can use !listjsons to see if the one you uploaded is there."
        + "\n  Lastly use the !updatedb command and your file will be processed and the bot will update player stats.";

        public readonly string MainHelpString = 
        "If you want help with a specific command use !help {yourcommand} to get information about the command."
        + " Otherwise, look below and react with the relevant emote to get info on that part of the bot."
        + $"\n For help with basic audio commands react with the {"1️⃣"} or use !audiohelp."
        + $"\n For help with database tracking react with the {"2️⃣"} or use !trackinghelp."
        + $"\n For help with searching the database react with the {"3️⃣"} or use !searchhelp."
        + $"\n To see a list of all commands use react with {"4️⃣"} !commandshelp.";

        public readonly string DBSearchHelpString = 
        "First, use a command like !searchdb or !searchop and type out the name of a show or some substring of a show. For example, " +
        "!searchop naruto will return all openings the bot has in the database from any show that contains naruto."
        +" If you know the exact name of a show, "+
        "you can use the exact variant of these commands like this !searchdbexact, !searchopexact." +
        "\n List of all search commands:\n !searchdb and !searchdbexact\n !searchop and !searchopexact\n "+
        "!searched and !searchedexact\n !searchins and !searchinsexact\n !searchartist and !searchartistexact.";
        
        
        public readonly Embed AudioHelpEmbed;
        public readonly Embed DatabaseTrackingHelpEmbed;

        public HelpEmbeds()
        {

        }
    }
}
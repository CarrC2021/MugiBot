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

        public readonly string AudioHelp2String =
        "Before using any of these commands I recommend getting used to the search commands found in !searchhelp. "
        + "If you would like to load all songs by a certain show or artist into a playlist use the following commands."
        + "\n !loadartist {artistname} \n !loadshow {show name} \n !loadshowexact {exact show name}"
        + "\n Similar to the searching commands there are op, ed, ins variants. Use them as seen below,"
        + "\n !loadshowops {show name} \n !loadshoweds {show name} \n !loadshowins {show name}"
        + "\nAll of these have exact variants called !loadshowopsexact, !loadshowedsexact, and !loadshowinsexact respectively."
        + "\nEach command will only load these songs into a playlist, in order for the music to actually play use !startradio.";

        public readonly string RadioHelpString = 
        "In order to use the radio you should first join a voice channel with !join. \n"
        + "If you want the bot to start playing songs from your list you can use !rcp {your user name}."
        + "Note you can also set the bot to use multiple people's list. An example is the following !rcp dingster bluegiraffetongue."
        + "You can then set the radio to play only the type of songs you are interested in hearing, see the output of !rlt for more information."
        + "If you want to set the radio to only play a certain type on your list use the command !rdl to remove different list types."
        + "One example is !rdl Watching Paused. After executing that command the radio will be set to play only shows that are completed, dropped, or planning."
        + "To add certain list types back use the command !ral. One example is !ral watching, that will make sure the radio will play shows it thinks you are watching."
        + "Use the command !printradio to print out all the current settings for the radio."
        + "Some important things to note are that I am currently working on anilist/mal/kitsu support, I know it is not fully working properly."
        + "Please help to improve the experience by using the songlistui script hosted on joseph98's github page to upload data to the bot."
        + "As more data comes in there will be a better radio experience. Lastly, update your anilist with !updateal to get all the new changes to your anilist.";

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
        + $"\n For help with basic audio commands use !audiohelp."
        + $"\n For help with more advanced audio commands use !audiohelp2"
        + $"\n For help with database tracking use !trackinghelp."
        + $"\n For help with searching the database use !searchhelp."
        + "\n For help with the radio use !radiohelp."
        + $"\n To see a list of all commands use !commandshelp.";

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
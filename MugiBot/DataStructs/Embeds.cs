using Discord;
using Discord.Webhook;
using PartyBot.Handlers;

namespace PartyBot.DataStructs
{
    public class HelpEmbeds
    {
        public readonly string AudioHelpString =
        " Step 1: Join a voice channel and then use the !join command to get the bot to join the voice channel.\n\n"
        + "Step 2 Option 1: You can play youtube videos, mp3 links, etc with the !play command.\n"
        + " (Note, if you do not type a direct link after the !play, it will search youtube and play some video.)\n\n"
        + "Step 2 Option 2: If you do not know the link to a catbox song, you can use any of the search commands and they will"
        + "give you something called a key. Just copy the key to the song you want and then use !playkey {copied key} to play them.\n"
        + " (Additionally, you can react to the embed mugibot puts out from the search command with some number x in 1-10 and it will play the x-th song in that embed).\n\n"
        + "Step 3 Managing the queue: If you don't like the current song use !skip to skip that song.\n To see a list of what is currently playing and queued use !queue."
        + "\n To pause the playback use !pause, to resume the playback use !resume.\n To get the bot to stop playing and clear the queue, use !stop."
        + "\n To get the bot to leave the voice channel use !leave.";

        public readonly string AudioHelp2String =
        "Before using any of these commands I recommend getting used to the search commands found in !searchhelp. "
        + "If you would like to load all songs by a certain show or artist into the queue use the following commands."
        + "\n !loadartist {artistname} \n !loadshow {show name} \n !loadshowexact {exact show name}"
        + "\n Similar to the searching commands there are op, ed, ins variants. Use them as seen below,"
        + "\n !loadshowops {show name} \n !loadshoweds {show name} \n !loadshowins {show name}"
        + "\nAll of these have exact variants called !loadshowopsexact, !loadshowedsexact, and !loadshowinsexact respectively."
        + "\nEach command will only load these songs into the queue, in order for the music to actually play use !startradio.";

        public readonly string RadioHelpString = 
        "Step 1: Confirm the bot has your list or not with !printlists.\n\n"
        + " Step 2: If you do not see your mal or anilist then you can do !updateal {your list} or !updatemal {your list} to get them.\n"
        + " (Note that the above command will fail if those lists are set to private.)\n\n"
        + " Step 3: As usual have the bot join the voice channel with !join.\n\n"
        + " Step 4: You can now set the radio using !rcp {your list name}, you can also set multiple with the same command by doing !rcp {some list} {another list} {this list}.\n"
        + " (The above command can take any finite number of lists so feel free to go wild. It can also do anilists or mal lists together, it does not matter.)\n\n"
        + " Step 5: Familiarize yourself with the default settings by running !rinfo.\n\n"
        + " Step 6: If you see types of songs you don't like you can run !rct {input}, if you see list types you don't want you can run !rdl {type to remove}.\n"
        + " Example uses: !rdl dropped -> will remove songs from the dropped part of the lists, !rct opening ending insert -> will change the bot to play all songs, "
        + " !rct opening -> the bot will only play openings. (rct also has an option to enter !rct {some number 1-6}, just experiment to see what setting corresponds to what number).\n\n"
        + " Step 7: Once the settings are the way you want them use !startradio to enjoy the show. Don't worry about input !startradio doesn't need any." 
        + " 👍.";
        public readonly string DatabaseTrackingHelpString = 
        "If you want to track a player's stats in the database use the command !addplayer."
        + " Here is an example of how to use it, !addplayer dingster A_Real_Dingus\n."
        + "\tWhat that does is track the amq user name dingster in the database as A_Real_Dingus."
        + " This is useful for when you have smurfs and want multiple amq user names under the same name in the database.\n\n"
        + " To see the players the bot is tracking use the !listplayers command.\n\n"
        + " In order to update the database you first have to upload jsons exported using Joseph98's song list ui script."
        + " Make sure the channel you upload to is named file-uploads otherwise the bot will ignore it.\n\n"
        + " To confirm the bot has downloaded the file you can use !listjsons to see if the one you uploaded is there.\n\n"
        + " If you want to remove a json for some reason use !deletejson {name of the json you uploaded}.\n\n"
        + " Lastly use the !updatedb command and your file will be processed and the bot will update player stats.";

        public readonly string MainHelpString = 
        "If you want help with a specific command use !help {yourcommand} to get information about the command."
        + " Otherwise, look below and use the relevant command."
        + $"\n For help with basic audio commands use !audiohelp."
        + $"\n For help with more advanced audio commands use !audiohelp2\n"
        + $" For help with database tracking use !trackinghelp."
        + $"\n For help with searching the database use !searchhelp."
        + "\n For help with the radio use !radiohelp.\n"
        + $" To see a list of all commands use !commandshelp.";

        public readonly string DBSearchHelpString = 
        "Step 1: Use a command like !searchdb or !searchop and type out the name of a show or some substring of a show.\n\n" 
        + " For example, !searchop naruto will return all openings the bot has in the database from any show that contains naruto.\n\n"
        +" Optional: If you know the exact name of a show, you can use the exact variant of these commands like this !searchdbexact, !searchopexact.\n\n" 
        +" Optional: If you want to search by author, you can use these commands !searchartist, !searchartistexact.\n\n" 
        + "Step 2: Familiarize yourself with the list of all search commands:\n !searchdb and !searchdbexact\n !searchop and !searchopexact\n "
        + "!searched and !searchedexact\n !searchins and !searchinsexact\n !searchartist and !searchartistexact.";
        
        
        public readonly Embed AudioHelpEmbed;
        public readonly Embed DatabaseTrackingHelpEmbed;

        public HelpEmbeds()
        {

        }
    }
}
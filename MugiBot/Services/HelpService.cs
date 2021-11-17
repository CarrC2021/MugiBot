using Discord;
using Discord.Commands;
using PartyBot.Handlers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PartyBot.DataStructs;

namespace PartyBot.Services
{
    public sealed class HelpService
    {
        private readonly CommandService _commands;
        private List<CommandInfo> _commandList;
        private readonly HelpEmbeds help;

        public HelpService(CommandService c)
        {
            _commands = c;
            _commandList = _commands.Commands.ToList();
            help = new HelpEmbeds();
        }

        public async Task<Embed> GetSummary(string search, CommandService cs)
        {
            string s = "";
            bool b = false;
            foreach (CommandInfo c in cs.Commands.ToList())
            {
                if (c.Name.ToLower().Equals(search.ToLower()))
                {
                    b = true;
                    s = s + c.Name + "\n" + c.Summary + "\n\n";
                }
            }
            if (b) return await EmbedHandler.CreateBasicEmbed("Help", s, Color.Blue);
            return await EmbedHandler.CreateBasicEmbed("Error", "Command not found", Color.Blue);
        }

        public async Task<Embed> AllCommands(CommandService cs)
        {
            string s = "";
            foreach (CommandInfo c in cs.Commands.ToList())
            {
                s = s + c.Name + "\n";
            }
            s = s + "\n" + "For more info on a specific command, call !help [COMMAND]";
            return await EmbedHandler.CreateBasicEmbed("List of commands:", s, Color.Blue);
        }

        public async Task<Embed> MainHelp()
        {
            var embed = await EmbedHandler.CreateBasicEmbed("Main Help", help.MainHelpString, Color.Green);
            return await EmbedHandler.CreateBasicEmbed("Main Help", help.MainHelpString, Color.Green);
        }

        public async Task<Embed> AudioHelp()
        {
            return await EmbedHandler.CreateBasicEmbed("Audio Help", help.AudioHelpString, Color.Green);
        }

        public async Task<Embed> DatabaseTrackingHelp() 
        {
            return await EmbedHandler.CreateBasicEmbed("Database Help", help.DatabaseTrackingHelpString, Color.Green);
        }

        public async Task<Embed> DatabaseSearchHelp()
        {
            return await EmbedHandler.CreateBasicEmbed("Database Search Help", help.DBSearchHelpString, Color.Green);
        }

        public async Task<Embed> RadioHelp()
        {
            return await EmbedHandler.CreateBasicEmbed("Radio Help", help.RadioHelpString, Color.Green);
        }
    }
}

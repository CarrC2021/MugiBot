using Discord;
using Discord.Commands;
using PartyBot.Handlers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyBot.Services
{
    public sealed class HelpService
    {
        private readonly CommandService _commands;
        private List<CommandInfo> _commandList;

        public HelpService(CommandService c)
        {
            _commands = c;
            _commandList = _commands.Commands.ToList();
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
    }
}

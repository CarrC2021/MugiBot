### A Music and DataBase Bot that functions as a tool for improving at AMQ. Designed For Medium Sized Servers.

## Built With

* [DotNet Core (Version - 3.1)](https://dotnet.microsoft.com/download/dotnet-core/3.1) - Dotnet version.
* [Discord.Net (Version - 2.3.1)](https://github.com/RogueException/Discord.Net) - The Discord Library used
* [Victoria (Version - 5.1.11)](https://github.com/Yucked/Victoria) - LavaLink Library.


### NOTE: This Requires At-Least C# Version 8.0

## Setting the bot up
1. Clone the repo
2. In a terminal type dotnet restore
3. Next launch the bot and it will automatically create a config.json file, edit this to have the correct discord token.
5. It should be able to connect to servers now

## To have the bot play music make sure to have a lavalink server open.
1. First use a terminal and cd into the directory containing a Lavalink.jar file
2. Then run java -jar Lavalink.jar
3. After that launch the bot, it should automatically connect to the lavalink server if the application.yml file is correct.

### To get the database working you  will need to install the dotnet entity framework core. 
1. In a terminal cd into the folder that contains program.cs and run dotnet tool install --global dotnet-ef
2. Next run dotnet ef migrations add initial
3. Finally run dotnet ef database update. You should cd into the Database folder to confirm that you have MugiBotDatbase.db

## Authors

* **Draxis** - *Initial work* - [Drax](https://github.com/joelp53/)
* **CarrC2021** - *AMQBot fork work* - https://github.com/CarrC2021
* **enslo22** - *AMQBOT fork work* - https://github.com/enslow22

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Yucked for Victoria.
* Egerod for developing Anime Music Quiz
    Their patreon can be found here https://www.patreon.com/NextWorldGames
* TheJoseph98 for making the SongListUI script which makes the database possible.
    * The script can be found here https://github.com/TheJoseph98/AMQ-Scripts
* blissfulyoshi for compiling the data found here https://gist.github.com/blissfulyoshi
* Catbox.moe, the main place that songs are hosted. 
    * Support Catbox by taking a look at the Patreon here https://www.patreon.com/catbox

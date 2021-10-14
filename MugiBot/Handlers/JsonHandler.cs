using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using PartyBot.DataStructs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PartyBot.Handlers
{
    public class JsonHandler
    {
        public static async Task DownloadJson(SocketMessage message, string JsonFolder)
        {
            char separator = Path.DirectorySeparatorChar;
            string fileName = "";
            try
            {
                for (int i = 0; i < message.Attachments.Count; i++)
                {
                    fileName = message.Attachments.ElementAt(i).Filename;
                    if (fileName.EndsWith("json"))
                    {
                        //Create a WebClient and download the attached file
                        using var client = new WebClient();

                        if (fileName.ToLower().Contains("expand library") || fileName.ToLower().Contains("expandlibrary"))
                            await Task.Run(() => client.DownloadFileAsync(new Uri(message.Attachments.ElementAt(i).Url),
                                Path.Combine(JsonFolder.Replace($"{separator}LocalJson", ""), fileName)));
                        else
                            await Task.Run(() => client.DownloadFileAsync(new Uri(message.Attachments.ElementAt(i).Url),
                                Path.Combine(JsonFolder, fileName)));
                        client.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex.Source);
            }
            Console.WriteLine("Downloads: Does this look like the file you wanted to download? " + fileName);
        }

        //asynchronously converts the Json file to a SongData object
        public static async Task<List<SongData>> ConvertJsonToSongData(FileInfo info)
        {
            string contents = File.ReadAllText(info.FullName);
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var data = await Task.Run(() =>
                JsonConvert.DeserializeObject<List<SongData>>(contents, settings));
            return data;
        }
        public static async Task<AMQExpandData> ConvertJsonToAMQExpandData(FileInfo info)
        {
            string contents = File.ReadAllText(info.FullName);
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var data = await Task.Run(() =>
                JsonConvert.DeserializeObject<AMQExpandData>(contents, settings));
            return data;
        }
        public static async Task<List<SongListData>> ConvertJsonToSongList(FileInfo info)
        {
            string contents = File.ReadAllText(info.FullName);
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var data = await Task.Run(() =>
                JsonConvert.DeserializeObject<List<SongListData>>(contents, settings));
            return data;
        }
        public static async Task<List<SongListData>> ConvertSongJsons(List<string> jsons)
        {
            List<SongListData> songs = new List<SongListData>();
            foreach (string contents in jsons)
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                var data = await Task.Run(() =>
                    JsonConvert.DeserializeObject<List<SongListData>>(contents, settings));
                songs.AddRange(data);
            }
            return songs;
        }

        public static async Task<FileInfo[]> GetAllJsonInFolder(string JsonFolder)
        {
            DirectoryInfo di = new DirectoryInfo(JsonFolder);
            var fileNames = await Task.Run(() => di.GetFiles("*.json"));
            Console.WriteLine(fileNames.Length);
            return fileNames;
        }

        public static async Task<Embed> ListJsons(string JsonFolder)
        {
            Console.WriteLine(JsonFolder);
            var filenames = await GetAllJsonInFolder(JsonFolder);

            string toPrint = "";
            foreach (var item in filenames)
            {
                toPrint += $"{item.Name}\n";
            }

            return await EmbedHandler.CreateBasicEmbed("Data", toPrint, Color.Blue);
        }
    }
}


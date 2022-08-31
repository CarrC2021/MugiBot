using System.Linq;
using Discord;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Anilist4Net;
using System.IO;
using System.Net;
using PartyBot.Queries;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using PartyBot.Handlers;
using PartyBot.DataStructs;
using PartyBot.Database;
using Microsoft.EntityFrameworkCore;

namespace PartyBot.Services
{
    public class AnilistService
    {
        private Anilist4Net.Client _anilistClient;
        private GraphQLHttpClient _graphQLClient { get; set; }
        private readonly char separator = Path.DirectorySeparatorChar;
        public string path { get; set; }
        private readonly Dictionary<string, string> FolderToExtension;
        private readonly Dictionary<string, int> ListStatusConversion;
        private JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public AnilistService()
        {
            _anilistClient = new Client(new HttpClient());

            FolderToExtension = new Dictionary<string, string>
            {
                {"CoverImages", ".png"},
                {"AniLists", ".json"},
                {"Statistics", ".json"},
                {"MediaFiles", ".json"}
            };
            ListStatusConversion = new Dictionary<string, int>
            {
                { "current", 1 },
                { "completed", 2 },
                { "paused", 3 },
                { "dropped", 4 },
                { "planning", 5 },
                { "repeating", 2}
            };
            var options = new GraphQLHttpClientOptions
            {
                EndPoint = new Uri("https://graphql.anilist.co"),
            };
            _graphQLClient = new GraphQLHttpClient(options, new SystemTextJsonSerializer(), new HttpClient());
        }

        public AnilistService(string rootPath)
        {
            _anilistClient = new Client(new HttpClient());
            path = rootPath;
            FolderToExtension = new Dictionary<string, string>
            {
                {"CoverImages", ".png"},
                {"AniLists", ".json"},
                {"Statistics", ".json"},
                {"MediaFiles", ".json"}
            };
            var options = new GraphQLHttpClientOptions
            {
                EndPoint = new Uri("https://graphql.anilist.co"),
            };
            _graphQLClient = new GraphQLHttpClient(options, new SystemTextJsonSerializer(), new HttpClient());
        }
        public async Task<string> GetCoverArtAsync(string show, int annId)
        {
            Media response = await GetMediaAsync(show);
            Console.WriteLine(response.Title.ToString());
            return response.CoverImageLarge;
        }
        public async Task<Embed> GetUserListAsync(ulong userID)
        {
            using var db = new AMQDBContext();
            var user = await db.DiscordUsers.FindAsync(userID);
            var username = user.AnilistName;
            var query = "query ($username: String){" + $"{AnilistQuery.MediaListQuery()}" + "}";
            var request = new GraphQLRequest { Query = query, Variables = new { username } };
            Console.Write(request);
            var response = await _graphQLClient.SendQueryAsync<dynamic>(request).ConfigureAwait(false);
            var UserList = response.Data.ToString();
            await WriteJsonResponseToFile(UserList, "AniLists", username);
            return await EmbedHandler.CreateBasicEmbed("Anilist", $"Downloaded a file containing the contents of {username}'s anilist", Color.Green);
        }

        public async Task<Embed> GetUserListAsync(string anilistName)
        {
            var username = anilistName;
            var query = "query ($username: String){" + $"{AnilistQuery.MediaListQuery()}" + "}";
            var request = new GraphQLRequest { Query = query, Variables = new { username } };
            Console.Write(request);
            var response = await _graphQLClient.SendQueryAsync<dynamic>(request).ConfigureAwait(false);
            var UserList = response.Data.ToString();
            await WriteJsonResponseToFile(UserList, "AniLists", username);
            return await EmbedHandler.CreateBasicEmbed("Anilist", $"Downloaded a file containing the contents of {username}'s anilist", Color.Green);
        }

        public async Task DownloadMediaAsync(string Show, string folder, int annId)
        {
            Media media = await GetMediaAsync(Show);
            await DownloadFromURL(media.SiteUrl, folder, $"{annId}");
        }
        public async Task<Media> GetMediaAsync(string Show)
        {
            Media response = null;
            try
            {
                response = await _anilistClient.GetMediaBySearch(Show);
            }
            catch (Exception ex)
            {
                await LoggingService.LogAsync(ex.Source, LogSeverity.Verbose, ex.Message, ex);
                Console.WriteLine(ex.StackTrace, ex.Message);
            }
            return response;
        }
        public async Task DownloadFromURL(string urlToDownload, string folder, string fileName)
        {
            using var client = new HttpClient();
            HttpResponseMessage responseMessage = await client.GetAsync(urlToDownload);
            responseMessage.EnsureSuccessStatusCode();
            var body = await responseMessage.Content.ReadAsStringAsync();
            Console.WriteLine(body);
            using var wc = new WebClient();
            string jsonResponse = await wc.DownloadStringTaskAsync(new Uri(urlToDownload));
            await WriteJsonResponseToFile(jsonResponse, folder, fileName);
        }

        public async Task WriteJsonResponseToFile(string jsonResponse, string folder, string fileName)
        {
            string localpath = Path.Combine(GlobalData.Config.RootFolderPath, $"{folder}", $"{fileName}{FolderToExtension[folder]}");
            await File.WriteAllTextAsync(localpath, jsonResponse);
        }

        public async Task<UserAnilist> ReturnUserAnilistAsync(string name, ulong id)
        {
            string fileLocation = Path.Combine(GlobalData.Config.RootFolderPath, "AniLists", $"{name}{FolderToExtension["AniLists"]}");
            if (!File.Exists(fileLocation))
                await GetUserListAsync(id);
            var userlist = JsonConvert.DeserializeObject<UserAnilist>(await File.ReadAllTextAsync(fileLocation), settings);
            return userlist;
        }
        /// <summary>
        /// This is an asynchronous function that returns all <see cref="SongTableObject>"/> objects from a 
        /// list of <see cref="PartyBot.DataStructs.UserAnilist>"/>. It uses the parameter validListNums to filter
        /// out songs not to be selected such as when the user does not want to retrieve watching shows or dropped shows.
        /// <summary>
        /// <param name="anilists"> A list of <see cref="PartyBot.DataStructs.UserAnilist>"/> containing all relevant data from the anilist of the user.
        /// </param>
        /// <param name="validListNums"> A list of <see cref="Int32"/> representing the desired list status of shows.
        /// </param>
        /// <returns> a list of <see cref="PartyBot.Database.SongTableObject>"/> objects once the asynchronous task is completed. </returns>
        public async Task<List<SongTableObject>> ReturnSongsFromList(UserAnilist anilist, List<int> validListNums)
        {
            var entries = new List<Entry>();
            var SongsToReturn = new List<SongTableObject>();
            for (int i = 0; i < anilist.MediaListCollection.Lists.Count; i++)
                entries.AddRange(anilist.MediaListCollection.Lists[i].Entries);
            
            // Get rid of the shows which are not the correct type, (planning, watching, completed, etc.)
            entries = entries.Where(x => validListNums.Contains(ListStatusConversion[x.Status.ToLower()])).ToList();
            using var db = new AMQDBContext();
            foreach (Entry entry in entries)
            {
                var media = await db.AnimeRelationalMaps
                            .AsNoTracking()
                            .Where(x => x.AnilistID.Equals(entry.Media.Id))
                            .ToListAsync();
                if (media.FirstOrDefault() != null)
                {
                    var tempList = await db.SongTableObject
                        .AsNoTracking()
                        .Where(y => y.AnnID == media.FirstOrDefault().AnnID)
                        .ToListAsync();
                    SongsToReturn.AddRange(tempList);
                }
            }
            return SongsToReturn;
        }

        /// <summary>
        /// This is an asynchronous function that returns all <see cref="SongTableObject>"/> objects from a 
        /// list of <see cref="PartyBot.DataStructs.UserAnilist>"/>. It uses the parameter validListNums to filter
        /// out songs not to be selected such as when the user does not want to retrieve watching shows or dropped shows.
        /// <summary>
        /// <param name="anilists"> A list of <see cref="PartyBot.DataStructs.UserAnilist>"/> containing all relevant data from the anilist of the user.
        /// </param>
        /// <param name="validListNums"> A list of <see cref="Int32"/> representing the desired list status of shows.
        /// </param>
        /// <returns> a list of <see cref="PartyBot.Database.SongTableObject>"/> objects once the asynchronous task is completed. </returns>
        public async Task<List<SongTableObject>> ReturnSongsFromLists(List<UserAnilist> anilists, List<int> validListNums)
        {
            var entries = new List<Entry>();
            var SongsToReturn = new List<SongTableObject>();
            foreach (UserAnilist anilist in anilists)
                SongsToReturn.AddRange(await ReturnSongsFromList(anilist, validListNums));
            return SongsToReturn;
        }
    }
}
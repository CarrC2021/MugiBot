using System.Linq;
using System.Net.Http.Headers;
using System.Net.Mime;
using Discord;
using PartyBot.Database;
using PartyBot.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Victoria;
using System.Collections.Generic;
using Newtonsoft.Json;
using Anilist4Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;
using PartyBot.Queries;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Serializer.SystemTextJson;
using PartyBot.DataStructs;
using PartyBot.Handlers;

namespace PartyBot.Services
{
    public class AnilistService
    {
        private Anilist4Net.Client _anilistClient;
        private GraphQLHttpClient _graphQLClient { get; set; }
        private readonly char separator = Path.DirectorySeparatorChar;
        public string path { get; set; }
        private readonly Dictionary<string, string> FolderToExtension;
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
        public async Task<Embed> GetUserListAsync(string username)
        {
            var query = "query ($username: String){"+ $"{AnilistQuery.MediaListQuery()}" +"}";
			var request = new GraphQLRequest {Query = query, Variables = new {username}};
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
            string localpath = Path.Combine(path, $"{folder}", $"{fileName}{FolderToExtension[folder]}");
            await File.WriteAllTextAsync(localpath, jsonResponse);
        }
    }
}
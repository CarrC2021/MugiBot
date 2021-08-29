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

namespace PartyBot.Services
{
    public class AnilistService
    {
        private Anilist4Net.Client _anilistClient;
        private readonly char separator = Path.DirectorySeparatorChar;
        private readonly string path;
        private readonly Dictionary<string, string> FolderToExtension;
        private JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };
        private HttpClient _client = new HttpClient();
        public AnilistService()
        {
            path = Path.GetDirectoryName(System.Reflection.
            Assembly.GetExecutingAssembly().GetName().CodeBase).Replace($"{separator}bin{separator}Debug{separator}netcoreapp3.1", "").Replace($"file:{separator}", "");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                path = separator + path;
            _anilistClient = new Client(new HttpClient());

            FolderToExtension = new Dictionary<string, string>
            {
                {"CoverImages", ".png"},
                {"AniLists", ".json"},
                {"Statistics", ".json"},
                {"MediaFiles", ".json"}
            };
        }
        public async Task<string> GetCoverArtAsync(string show, int annId)
        {
            Media response = await GetMediaAsync(show);
            Console.WriteLine(response.Title.ToString());
            return response.CoverImageLarge;
        }
        public async Task GetUserAsync(string userName)
        {
            var user = await _anilistClient.GetUserByName(userName);
            string query = AniListQueryCreator.MediaListQuery(user.Name);
            Console.WriteLine(query);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://graphql.anilist.co"),
                Content = new StringContent(query)
            };
            using var client = new HttpClient();
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine(body);
            await DownloadFromURL(user.SiteUrl + "/animelist", "AniLists", user.Name);

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
            string localpath = Path.Combine(path, $"{folder}", $"{fileName}.{FolderToExtension[folder]}");
            using var wc = new WebClient();
            string jsonResponse = await wc.DownloadStringTaskAsync(new Uri(urlToDownload));
            await File.WriteAllTextAsync(localpath, jsonResponse);
        }
    }
}
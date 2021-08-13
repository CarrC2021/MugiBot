using System.Text;
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

namespace PartyBot.Services
{
    public class AnilistService
    {
        private string myEndPoint = "";
        private Anilist4Net.Client _anilistClient;
        private HttpClient httpClient = new HttpClient();
        private readonly char separator = Path.DirectorySeparatorChar;
        private readonly string path;
        public AnilistService()
        {
            path = Path.GetDirectoryName(System.Reflection.
            Assembly.GetExecutingAssembly().GetName().CodeBase).Replace($"{separator}bin{separator}Debug{separator}netcoreapp3.1", "").Replace($"file:{separator}", "");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                path = separator + path;
            _anilistClient = new Client(new HttpClient());
        }
        public void SetEndpoint(string endPoint)
        {
            myEndPoint = endPoint;
        }
        public async Task SearchSeriesOnAL(string show, int annId)
        {
            Media response = null;
            try
            {
                response = await _anilistClient.GetMediaBySearch(show);
                Console.WriteLine(response.CoverImageLarge);
                using var client = new HttpClient();
                HttpResponseMessage responseMessage = await client.GetAsync(response.CoverImageLarge);
                responseMessage.EnsureSuccessStatusCode();

                int LastIndexOf = response.CoverImageLarge.LastIndexOf("/");
                Console.WriteLine(LastIndexOf);
                string cutString = response.CoverImageLarge.Substring(LastIndexOf-1);
                Console.WriteLine(cutString);

                string localpath = Path.Combine(path, "CoverImages", $"{annId}.png");

                using var wc = new WebClient();
                Uri tempurl = new Uri(response.CoverImageLarge);
                var audio = await wc.DownloadDataTaskAsync(tempurl);
                File.WriteAllBytes(localpath, audio);
            }
            catch (Exception ex)
            {
                await LoggingService.LogAsync(ex.Source, LogSeverity.Verbose, ex.Message, ex);
                Console.WriteLine(ex.StackTrace, ex.Message);
            }
            Console.WriteLine(response.Title.ToString());
        }
    }
}
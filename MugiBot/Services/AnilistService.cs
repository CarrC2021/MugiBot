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

namespace PartyBot.Services
{
    public class AnilistService
    {
        private string myEndPoint = "";
        private Anilist4Net.Client _anilistClient;
        public AnilistService()
        {
            _anilistClient = new Client(new HttpClient());
        }
        public void SetEndpoint(string endPoint)
        {
            myEndPoint = endPoint;
        }
        public async Task SearchSeriesOnAL(string show)
        {
            StringBuilder sb = new StringBuilder("query($page: 1, $perPage: 10," + $"$search: {show})");
            sb.Append("{Page(page: $page, perPage: $perPage) {pageInfo {" + 
                    "total\n" +
                    "currentPage\n" +
                    "lastPage\n" +
                    "hasNextPage\n" +
                    "perPage\n"+
                    "})");
            sb.Append("media(search: $search, source: ANIME) {"+
                    "id\n"+
                    "title {\n"+
                        "romaji\n"+
                        "english\n"+
                        "native\n"+
                    "}\n"+
                    "coverImage\n"+
                    "bannerImage\n"+
                    "}}}"
                    );

            Console.WriteLine(sb.ToString());
            Media response = null;
            MediaCoverImage image = null;
            try
            {
                response = await _anilistClient.GetMediaBySearch(show);
                Console.WriteLine(response.CoverImageLarge);
                image = response.CoverImage;
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
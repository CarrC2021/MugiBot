using System.Text;
using Discord;
using PartyBot.Database;
using PartyBot.Services;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Victoria;
using System.Collections.Generic;
using Miki.GraphQL;
using Miki.Net;
using Newtonsoft.Json;

namespace PartyBot.Services
{
    public class AnilistService
    {
        private string myEndPoint = "";
        private GraphQLClient _client;
        public AnilistService()
        {

        }
        public AnilistService(string endPoint)
        {
            myEndPoint = endPoint;
            _client = new GraphQLClient(myEndPoint);
        }
        public void SetEndpoint(string endPoint)
        {
            myEndPoint = endPoint;
            _client = new GraphQLClient(myEndPoint);
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
                    "bannerImage\n"
                    );

            Console.WriteLine(sb.ToString());
            string response = await _client.QueryAsync(sb.ToString(), 22);
            Console.WriteLine(response);
            var finalResponse = JsonConvert.DeserializeObject(response);
        }
    }
}
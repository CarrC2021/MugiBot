using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PartyBot.Handlers
{
    public static class GithubHandler
    {
        public static async Task<List<string>> ReturnJsonGists(string user, int page, int numPerPage)
        {
            List<string> jsonData = new List<string>();
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("PartyBot", "1"));
            var contentsUrl = $"https://api.github.com/users/{user}/gists?page={page}&per_page={numPerPage}";
            var contentsJson = await httpClient.GetStringAsync(contentsUrl);
            var contents = (JArray)JsonConvert.DeserializeObject(contentsJson);
            //Console.WriteLine(contents);
            Console.WriteLine(contents.Count);
            foreach (var token in contents)
            {
                var thing = token["files"];
                //Console.WriteLine(thing);
                foreach (var entry in thing.Children())
                {
                    string filename = entry.First["filename"].ToString();
                    if (filename.Contains("Ranked Song List"))
                    {
                        //Console.WriteLine(entry.First["raw_url"]);
                        var rawURL = entry.First["raw_url"];
                        string val = rawURL.ToString();
                        var tempClient = new WebClient();
                        try
                        {
                            string text = await tempClient.DownloadStringTaskAsync(val);
                            jsonData.Add(text);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message, ex.Source, ex.StackTrace);
                        }
                    }
                }
            }
            return jsonData;
        }
    }
}


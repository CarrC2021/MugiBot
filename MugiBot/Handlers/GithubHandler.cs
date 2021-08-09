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
    /// <summary>
    /// A static class which will have functions to use the Github API.
    /// <summary>
    public static class GithubHandler
    {
        /// <summary>
        /// This is an asynchronous function to download files using the github gist api.
        /// There are three parameters. The first parameter is <param name="user"/> which 
        /// is the user who has posted the gist. The second is <param name="page"/> which is 
        /// the desired page of the request. The third is <param name="numPerPage"/>, the number
        /// of things from the page you want. 
        /// <returns> A list of strings which are the direct paths to the now downladed json files. </returns>
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


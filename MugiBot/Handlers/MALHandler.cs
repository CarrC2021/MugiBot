using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using PartyBot.DataStructs;
using Newtonsoft.Json;
using System.Collections.Generic;
using PartyBot.Database;
using System.Linq;
using Discord;
using Microsoft.EntityFrameworkCore;

namespace PartyBot.Handlers
{
    public class MALHandler
    {
        public static async Task GetMalUserList(string userName)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            
            string path = Path.Combine(GlobalData.Config.RootFolderPath, "malAPI.json");
            MalUserList tempList = new MalUserList(new List<Datum>(), new Paging($"https://api.myanimelist.net/v2/users/{userName}/animelist?fields=list_status&limit=1000"));

            MalUserList toReturn = await GetListData(GlobalData.Config.MalClientID, tempList);
            await File.WriteAllTextAsync(Path.Combine(GlobalData.Config.RootFolderPath, "MALUserLists", $"{userName}.json"), JsonConvert.SerializeObject(toReturn));
        }

        private static async Task<MalUserList> GetListData(string ClientId, MalUserList list)
        {
            if (list.Paging.Next==null || list.Paging.Next=="")
                return list;
            Console.WriteLine("Recursive Call");
            var GETREQUEST = WebRequest.Create(list.Paging.Next);
            GETREQUEST.Headers.Add("X-MAL-CLIENT-ID", ClientId);
            GETREQUEST.Credentials = CredentialCache.DefaultCredentials;
            var response = (HttpWebResponse)(await GETREQUEST.GetResponseAsync().ConfigureAwait(false));
            Console.WriteLine(response.StatusDescription);
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader (dataStream);
            string responseFromServer = reader.ReadToEnd();

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            MalUserList templist = JsonConvert.DeserializeObject<MalUserList>(responseFromServer, settings);

            // Add all the data returned from this page of the api call and then return another recursive call.
            Console.WriteLine(templist.Data[0].ListStatus.StartDate);
            list.Data.AddRange(templist.Data);
            list.Paging.Next = templist.Paging.Next;
            reader.Close ();
            dataStream.Close ();
            response.Close ();
            return await GetListData(ClientId, list);
        }

        public static async Task<Embed> UpdateUserListAsync(ulong userID)
        {
            using var db = new AMQDBContext();
            var user = await db.DiscordUsers.FindAsync(userID);
            if (user.MALName == null)
                return await EmbedHandler.CreateErrorEmbed("Mal User Lists", "Your Mal user name is not set");
            try
            {
                await GetMalUserList(user.MALName);
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Mal User Lists", "There was an error finding the MAL user you set. Check to make sure"
                + "you have spelled it correctly with !printmyinfo.");
            }
            return await EmbedHandler.CreateBasicEmbed("Mal User Lists", $"Downloaded a file containing the contents of {user.MALName}'s list.", Color.Green);
        }

        public static async Task<Embed> UpdateUserListAsync(string malName)
        {
            try
            {
                await GetMalUserList(malName);
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Mal User Lists", "There was an error finding the MAL user you set. Check to make sure"
                + "you have spelled it correctly with !printmyinfo.");
            }
            return await EmbedHandler.CreateBasicEmbed("Mal User Lists", $"Downloaded a file containing the contents of {malName}'s list.", Color.Green);
        }

        public static async Task<List<SongTableObject>> GetSongsFromMAL(string MALName, List<int> Status)
        {

            var toReturn = new List<SongTableObject>();
            var ListStatusConversion = new Dictionary<string, int>
            {
                { "watching", 1 },
                { "completed", 2 },
                { "on_hold", 3 },
                { "dropped", 4 },
                { "plan_to_watch", 5 },
            };
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            string fileLocation = Path.Combine(GlobalData.Config.RootFolderPath, "MALUserLists", $"{MALName}.json");
            var list = JsonConvert.DeserializeObject<MalUserList>(await File.ReadAllTextAsync(fileLocation), settings);

            // This reduced list will remove shows with undesired list status. 
            var reducedList = list.Data.Where(node => Status.Contains(ListStatusConversion[node.ListStatus.Status]));
            
            // Now we need to convert from MAL's unique ID to ANNID.
            using var db = new AMQDBContext();
            foreach (Datum data in reducedList)
            {
                var show = await db.AnimeRelationalMaps
                    .AsNoTracking()
                    .Where(entry => entry.MALID == data.Node.Id)
                    .ToListAsync();
                if (show.FirstOrDefault() != null)
                {
                    var tempList = await db.SongTableObject
                        .AsNoTracking()
                        .Where(y => y.AnnID == show.FirstOrDefault().AnnID)
                        .ToListAsync();
                    toReturn.AddRange(tempList);
                }
            }
            return toReturn;
        }
    }
}
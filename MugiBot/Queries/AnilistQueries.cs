using System.Text;
namespace PartyBot.Queries
{
    public static class AniListQueryCreator
    {
        public static string MediaListQuery(string userName)
        {
            var query = 
            "query{MediaListCollection(userName:"+$"{userName} "+"type:ANIME){"
                +"sort{"
                +    "MEDIA_ID"
                +"}}}";
            return query;
        }
    }
}
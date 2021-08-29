using System.Text;
namespace PartyBot.Queries
{
    public static class AniListQueryCreator
    {
        public static string MediaListQuery(string userName)
        {
            var query = 
            $"query({userName}: string)"+"{"
                +$"MediaListCollection(userName: {userName}, " + "type: ANIME){"
                +"sort{"
                +    "MEDIA_ID"
                +"}}}";
            return query;
        }

        public static string OtherMediaListQuery(string userName)
        {
            var query = 
            $"query({userName}: string)"+"{"
                + $"MediaListCollection(userName:{userName}, " + "type:ANIME){"
                +"sort{"
                +    "MEDIA_ID"
                +"}}}";
            return query;
        }
    }
}
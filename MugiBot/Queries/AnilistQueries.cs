using System.Text;
namespace PartyBot.Queries
{
    public static class AnilistQuery
    {
        public static string MediaListQuery()
        {
            var query =
            "MediaListCollection(userName: $username, type: ANIME){\n"
                +"lists{\n"
                    +"entries{\n"
                        +"status\n"
                        +"media{\n"
                        +"id\n"
                        +"idMal\n"
                        +"seasonYear\n"
                        +"title {\n"
                            +"romaji\n"
                            +"english\n"
                        +"}\n"
                        +"coverImage {\n"
                        +"large\n"
                        +"medium\n}\n"
                        +"genres\n"
                        +"tags{\n"
                        +"name\n description}\n"
                        +"}\n"
                    +"}\n"
                +"}\n"
            +"}";
            return query;
        }

        public static string OtherMediaListQuery(string userName)
        {
            var query =
            $"query({userName}: string)" + "{"
                + $"MediaListCollection(userName:{userName}, " + "type:ANIME){"
                + "sort{"
                + "MEDIA_ID"
                + "}}}";
            return query;
        }
    }
}
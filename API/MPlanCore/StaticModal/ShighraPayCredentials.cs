namespace MplanCore.StaticModal
{
    public class ShighraPayCredentials
    {
        public static string api_token = "";

        public static string user_id = "";

        internal const string PlanURL = "https://saarthi.shighrapay.com/recharge/shighrapay/plans";

        internal const string OperatorSearchURL = "https://saarthi.shighrapay.com/recharge/shighrapay/opsearch";

        public static void SetCredentials(string apitoken, string userid)
        {
            api_token = apitoken;
            user_id = userid;
        }
    }
}
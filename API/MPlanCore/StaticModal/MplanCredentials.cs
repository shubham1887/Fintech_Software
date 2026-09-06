namespace MplanCore.StaticModal
{
    public class MplanCredentials
    {
        public static string ApiKey = "";
        public static string Password = "";

        internal const string ROfferURL = "https://mplan.in/api/plans.php?apikey=[ApiKey]&offer=roffer&tel=[ConsumerNumber]&operator=[Operator]";

        internal const string PlanURL = "https://mplan.in/api/plans.php?apikey=[ApiKey]&cricle=[Circle]&operator=[Operator]";

        internal const string CircleApiURL = "http://planapi.in/api/Mobile/OperatorFetch?apimember_id=[apimember_id]&api_password=[api_password]&Mobileno=[Number]";

        internal const string DTHPlanURL = "https://mplan.in/api/dthplans.php?apikey=[ApiKey]&operator=[Operator]";

        internal const string CustomerInfoURL = "https://www.mplan.in/api/Dthinfo.php?apikey=[ApiKey]&offer=roffer&tel=[ConsumerNumber]&operator=[Operator]";

        internal const string HeavyRefreshURL = "https://www.mplan.in/api/Dthheavy.php?apikey=[ApiKey]&offer=roffer&tel=[ConsumerNumber]&operator=[Operator]";

        public static void SetCredentials(string key)
        {
            ApiKey = key;
        }
    }
}
namespace MplanCore.StaticModal
{
    public class InfoapiCredentials
    {
        public static string ApiKey = "";
        internal const string ROfferURL = "https://infoapi.in/api/mobile/roffer?access_token=[ApiKey]&cust=[ConsumerNumber]&operator=[Operator]";       
        internal const string PlanURL = "https://infoapi.in/api/mobile/plans/v2?access_token=[ApiKey]&cust=[ConsumerNumber]&operator=[Operator]";
        internal const string PostpaidBillInfoURL = "https://infoapi.in/api/mobile/bill?access_token=[ApiKey]&cust=[ConsumerNumber]&operator=[Operator]";
    
        internal const string DTHPlanURL = "https://infoapi.in/api/dth/plans/v2?access_token=[ApiKey]&cust=[ConsumerNumber]&operator=[Operator]";
        internal const string DTHROfferURL = "https://infoapi.in/api/dth/roffer?access_token=[ApiKey]&cust=[ConsumerNumber]&operator=[Operator]";

        internal const string ElectricityURL = "https://infoapi.in/api/electricity/bill?access_token=[ApiKey]&cust=[ConsumerNumber]&operator=[Operator]";

        //internal const string CustomerInfoURL = "https://www.mplan.in/api/Dthinfo.php?apikey=[ApiKey]&offer=roffer&tel=[ConsumerNumber]&operator=[Operator]";
        //internal const string HeavyRefreshURL = "https://www.mplan.in/api/Dthheavy.php?apikey=[ApiKey]&offer=roffer&tel=[ConsumerNumber]&operator=[Operator]";

        public static void SetCredentials(string key)
        {
            ApiKey = key;
        }
    }
}
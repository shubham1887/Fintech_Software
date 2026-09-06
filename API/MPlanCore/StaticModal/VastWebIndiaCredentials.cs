namespace MplanCore.StaticModal
{
    public class VastWebIndiaCredentials
    {
        public static string ApiUserID = "";

        public static string ApiToken = "";

        public const string BaseURL = "https://www.vastwebindia.com/Offer/";

        internal const string OpertorCheckURL = BaseURL + "GetInfo?userid=[ApiUserID]&token=[ApiToken]&number=[ConsumerNumber]";

        internal const string ROfferURL = BaseURL + "Roffer_check?userid=[ApiUserID]&token=[ApiToken]&optnm=[Operator]&number=[ConsumerNumber]";

        internal const string MobilePlanNameURL = BaseURL + "Plan_check?userid=[ApiUserID]&token=[ApiToken]&optnm=[Operator]&circlenm=Maharashtra&type=MOBILE";

        internal const string MobilePlanURL = BaseURL + "Plan_Details?userid=[ApiUserID]&token=[ApiToken]&optnm=[Operator]&circlenm=Maharashtra&type=MOBILE&name=[Name]";

        internal const string DTHPlanNameURL = BaseURL + "Plan_check?userid=[ApiUserID]&token=[ApiToken]&optnm=[Operator]&circlenm=Maharashtra&type=DTH";

        internal const string DTHPlanURL = BaseURL + "Plan_Details?userid=[ApiUserID]&token=[ApiToken]&optnm=[Operator]&circlenm=Maharashtra&type=DTH&name=[Name]";

        internal const string PlanDetailsURL = BaseURL + "Plan_Details?userid=[ApiUserID]&token=[ApiToken]&optnm=[Operator]&circlenm=Maharashtra&type=DTH&name=[Name]";

        internal const string CustomerInfoURL = BaseURL + "DTHCustomerinfo?userid=[ApiUserID]&token=[ApiToken]&optnm=[Operator]&number=[ConsumerNumber]";

        public static string ElectricityURL = BaseURL + "ElectricityViewBill?userid=[ApiUserID]&token=[ApiToken]&StateName=[StateName]&BoardName=[Operator]&account=[ConsumerNumber]&ConsumerNumber=[MobileNumber]";

        public static void SetCredentials(string UserID, string Token)
        {
            ApiUserID = UserID;
            ApiToken = Token;
        }
    }
}
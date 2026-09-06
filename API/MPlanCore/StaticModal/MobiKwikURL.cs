namespace MplanCore.StaticModal
{
    public class MobiKwikURL
    {
        //public static string BaseURL = "https://rapi.mobikwik.com";  //prod
        public static string BaseURL = "https://alpha3.mobikwik.com";  //UAT 

        public static string PlanFetchingGET = BaseURL + "/recharge/v1/rechargePlansAPI/[OperatorID]/[CircleID]";

        public static string PlanFetchingwithTypeGET = BaseURL + "/recharge/v1/rechargePlansAPI/[OperatorID]/[CircleID]/[PlanType]";

        //public static void SetCredentials(string UserID, string Password)
        //{
        //    ApiUserID = UserID;
        //    ApiPassword = Password;
        //}
    }
}

namespace MplanCore.StaticModal
{
    public class EzytmCredentials
    {
        public static string ApiUserID = "";

        public static string ApiPassword = "";

        internal const string OpertorCheckURL = "http://planapi.in/api/Mobile/OperatorFetchNew?ApiUserID=[ApiUserID]&ApiPassword=[ApiPassword]&Mobileno=[Mobile]";

        internal const string ROfferURL = "http://planapi.in/api/Mobile/RofferCheck?apimember_id=[ApiUserID]&api_password=[ApiPassword]&mobile_no=[ConsumerNumber]&operator_code=[Operator]";

        internal const string PlanURL = "http://planapi.in/api/Mobile/Operatorplan?apimember_id=[ApiUserID]&api_password=[ApiPassword]&cricle=[Circle]&operatorcode=[Operator]";

        internal const string DTHPlanURL = "http://planapi.in/api/Mobile/Operatorplan?apimember_id=[ApiUserID]&api_password=[ApiPassword]&cricle=0&operatorcode=[Operator]";

        internal const string CustomerInfoURL = "http://planapi.in/api/Mobile/DTHINFOCheck?apimember_id=[ApiUserID]&api_password=[ApiPassword]&Opcode=[Operator]&mobile_no=[ConsumerNumber]";

        public static string ElectricityURL = "http://planapi.in/api/Mobile/BillCheck?apimember_id=[ApiUserID]&api_password=[ApiPassword]&Accountno=[ConsumerNumber]&operatorcode=[Operator]&Mobileno=[MobileNumber]";

        public static void SetCredentials(string UserID, string Password)
        {
            ApiUserID = UserID;
            ApiPassword = Password;
        }
    }
}
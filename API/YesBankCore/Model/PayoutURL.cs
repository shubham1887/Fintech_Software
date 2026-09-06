using YESBankCore.Controller;
namespace YESBankCore.Model
{
    internal class PayoutURL
    {
        //prod
        public static readonly string BaseURL = "https://skyway.yesbank.in/app/live/";

        // Payout
        public static readonly string DomesticPayment = "api-banking/domestic-payments";

        public static readonly string PaymentStatus = "api-banking/payment-details";
        public static readonly string AccountBalance = "api-banking/fund-confirmation";

        public static string getURL(string action)
        {
            string fullURL = BaseURL + action;

            if (PayoutCredential.Mode.EqualsIgnoreCase("UAT"))
            {
                //dev
                string baseURLUAT = "https://uatskyway.yesbank.in/app/uat/";
                fullURL = baseURLUAT + action;
            }

            return fullURL;
        }
    }
}

using YESBankCore.Controller;

namespace YESBankCore.Model
{
    internal class URL
    {
        //prod
        public static readonly string BaseURL = "https://api.yespayhub.in/services/";

        //UPI PG
        public static readonly string QRCollect = "upi/logIntent";

        public static readonly string CheckVirtualAddress = "upi/checkVirtualAddressME";
        public static readonly string StatusQuery = "upi/meTransStatusQuery";
        public static readonly string FundTransfer = "disbursement/pd";
        public static readonly string VirtalAccountBalance = "partner/partner";
      
        public static string getURL(string action)
        {
            string fullURL = BaseURL + action;

            if (Credential.Mode.EqualsIgnoreCase("UAT"))
            {
                //dev
                string baseURLUAT = "https://uatskyway.yesbank.in/app/uat/";
                fullURL = baseURLUAT + action;
            }

            return fullURL;
        }
        public static string getPayoutURL(string action)
        {
            string fullURL = BaseURL + action;
            Credential.Mode = "UAT";

            if (Credential.Mode.EqualsIgnoreCase("UAT"))
            {
                //dev
                string baseURLUAT = "https://api.uatyespayhub.in/services/";
                fullURL = baseURLUAT + action;
            }

            return fullURL;
        }
    }
}
using YESBankCore.Controller;
using YESBankCore.Model;

namespace YesBankCore.Model
{
    internal class AEPSURL
    {
        public static readonly string BaseURL = "https://ybluat.transxt.in/dmtaeps-web"; // UAT

        // AEPS
        public static readonly string Authentication = "/api/1.0/enc/nonfin/reqAuth";
        public static readonly string CashWithdraw = "/api/1.0/enc/fin/cashWithdrawal";
        public static readonly string BalanceEnquiry = "/api/1.0/enc/nonfin/balanceEnquiry";
        public static readonly string Ministatement = "/api/1.0/enc/nonfin/ministatement";

        public static string getURL(string action)
        {
            string fullURL = BaseURL + action;

            if (AEPSCredentials.Mode.EqualsIgnoreCase("UAT"))
            {
                //dev
                string baseURLUAT = "https://ybluat.transxt.in/dmtaeps-web";
                fullURL = baseURLUAT + action;
            }

            return fullURL;
        }
    }
}

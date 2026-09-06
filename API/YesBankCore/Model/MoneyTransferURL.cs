using YESBankCore.Controller;

namespace YESBankCore.Model
{
    internal class MoneyTransferURL
    {
         public static readonly string BaseURL = "https://ybl.transxt.in/dmtsor-web"; // Production

        // DMT
        public static readonly string SelectSender = "/api/1.0/enc/customer/fetch";
        public static readonly string SenderRegistration = "/api/1.0/enc/customer/create";
        public static readonly string GenerateOTP = "/api/1.0/enc/otp/generateBeneAddOtp";
        public static readonly string AddBene = "/api/1.0/enc/beneficiary/create";
        public static readonly string GetAllBene = "/api/1.0/enc/beneficiary/fetch";
        public static readonly string DeleteBene = "/api/1.0/enc/beneficiary/activateDeActivate";      
        public static readonly string Transfer = "/api/1.0/enc/transaction/fundtransfer/dmt";
        public static readonly string StatusCheck = "/api/1.0/enc/transaction/fetchtxnstatus";
        public static readonly string BeneAccVerify = "/api/1.0/enc/benevalidation/nameinquiry";
        public static readonly string VerifyStatusCheck = "/api/1.0/enc/benevalidation/fetchstatus";    
        public static readonly string RefundOTP = "/api/1.0/enc/otp/generateRefundOtp";
        public static readonly string Refund = "/api/1.0/enc/transaction/refundtransfer";
        public static readonly string BeneVerifyRefundOTP = "/api/1.0/enc/benevalidation/generaterefundotp";
        public static readonly string BeneVerifyRefund = "/api/1.0/enc/benevalidation/refundtransfer";
     
        public static string getURL(string action)
        {
            string fullURL = BaseURL + action;

            if (MoneyTransferCredential.Mode.EqualsIgnoreCase("UAT"))
            {
                //dev
                string baseURLUAT = "https://ybluat.transxt.in/dmtsor-web";
                fullURL = baseURLUAT + action;
            }

            return fullURL;
        }

        public static string getURLForVerify(string action)
        {
            string fullURL = BaseURL + action;

            if (MoneyTransferCredential.Mode.EqualsIgnoreCase("UAT"))
            {
                //dev             
                string baseURLUAT = "https://ybluat.transxt.in/dmt-accountverification";
                fullURL = baseURLUAT + action;
            }

            return fullURL;
        }
    }
}

using System.Collections.Generic;

namespace Project.Response.IPWebSDK
{
    public class BalanceEnquiryResponse
    {
        public string response_code { get; set; }

        public string response_msg { get; set; }

        public List<Transaction_AEPS> transactions { get; set; }

        public BalanceEnquiryResponse()
        {
            transactions = new List<Transaction_AEPS>();
        }
    }

    public class Transaction_AEPS
    {
        public string balance { get; set; }
    }
}
using Project.Processing;

namespace Project.Response.Quickekyc
{
    public class UPIVerificationAPIResponse
    {
        public UPIData data { get; set; }
        public int status_code { get; set; }
        public string message { get; set; }
        public string status { get; set; }
        public int request_id { get; set; }
    }

    public class UPIData
    {
        public bool account_exists { get; set; }
        public string name_at_bank { get; set; }
        public NameInformation name_information { get; set; }
    }

    public class NameInformation
    {
        public string name_at_bank_cleaned { get; set; }
    }
}

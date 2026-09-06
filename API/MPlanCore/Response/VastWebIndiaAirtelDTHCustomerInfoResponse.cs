namespace MplanCore.Response
{
    public class VastWebIndiaAirtelDTHCustomerInfoResponse
    {
        public string status { get; set; }
        public AirtelDTHCustomerInfo Response { get; set; }
    }

    public class AirtelDTHCustomerInfo
    {
        public string name { get; set; }
        public string currentbalance { get; set; }
        public string validTill { get; set; }
    }
}
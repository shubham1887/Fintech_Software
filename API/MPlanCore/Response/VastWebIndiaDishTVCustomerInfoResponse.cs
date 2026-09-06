namespace MplanCore.Response
{
    public class VastWebIndiaDishTVCustomerInfoResponse
    {
        public string status { get; set; }
        public DishTVCustomerInfoResponse Response { get; set; }
    }

    public class DishTVCustomerInfoResponse
    {
        public string SubsBalance { get; set; }
        public string ActivatedOn { get; set; }
        public string StatusName { get; set; }
        public string SubscriberName { get; set; }
        public string address { get; set; }
        public string State { get; set; }
        public string SchemeName { get; set; }
        public string PaytermName { get; set; }
        public string PackagePrice { get; set; }
    }
}
namespace MplanCore.Response
{
    public class VastWebIndiaROfferResponse
    {
        public string status { get; set; }
        public List<ROffer> Response { get; set; }
    }

    public class ROffer
    {
        public string price { get; set; }
        public string offer { get; set; }
        public string offerDetails { get; set; }
        public string commAmount { get; set; }
        public string commType { get; set; }
    }
}
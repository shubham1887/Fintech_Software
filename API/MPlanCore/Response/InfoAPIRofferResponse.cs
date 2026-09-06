namespace MplanCore.Response
{
    public class InfoAPIRofferResponse
    {
        public string status { get; set; }
        public string message { get; set; }
        public string cust { get; set; }
        public string @operator { get; set; }
        public List<ROfferData> data { get; set; }
    }

    public class ROfferData
    {
        public string price { get; set; }
        public string commission { get; set; }
        public string description { get; set; }
    }
}
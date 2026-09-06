namespace MplanCore.Response
{
    public class DTHRofferResponse
    {
        public string status { get; set; }
        public string message { get; set; }
        public string cust { get; set; }
        public string @operator { get; set; }
        public List<string> data { get; set; }
    }
}
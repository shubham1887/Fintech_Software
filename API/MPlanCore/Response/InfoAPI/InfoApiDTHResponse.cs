namespace MplanCore.Response.InfoAPI
{
    public class InfoApiDTHResponse
    {
        public string status { get; set; }
        public string message { get; set; }
        public string cust { get; set; }
        public string @operator { get; set; }
        public DTHData data { get; set; }
    }

    public class DTHData
    {
        public List<string> categories { get; set; }
        //public List<List<Dth>> plans { get; set; }
    }
}

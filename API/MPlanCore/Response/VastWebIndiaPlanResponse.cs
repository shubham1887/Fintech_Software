namespace MplanCore.Response
{
    public class VastWebIndiaPlanResponse
    {
        public string status { get; set; }
        public List<MobileResponse> Response { get; set; }
    }

    public class MobileResponse
    {
        public string price { get; set; }
        public string description { get; set; }
        public string Talktime { get; set; }
        public string Validity { get; set; }
    }
}
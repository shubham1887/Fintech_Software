namespace MplanCore.Response
{
    public class VastWebIndiaPlanNameResponse
    {
        public string status { get; set; }
        public List<Response> Response { get; set; }
    }

    public class Response
    {
        public string name { get; set; }
    }
}
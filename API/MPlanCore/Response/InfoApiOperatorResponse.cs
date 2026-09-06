namespace MplanCore.Response
{
    public class InfoApiOperatorResponse
    {
        public string status { get; set; }
        public string message { get; set; }
        public string cust { get; set; }
        public Datas data { get; set; }
    }

    public class Datas
    {
        public int @operator { get; set; }
        public int state { get; set; }
    }
}
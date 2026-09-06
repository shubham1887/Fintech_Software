namespace MplanCore.Response
{
    public class ElectricityResponse
    {
        public string tel { get; set; }
        public string @operator { get; set; }
        public List<Record> records { get; set; }
        public int status { get; set; }
    }
}
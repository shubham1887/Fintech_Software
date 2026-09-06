namespace MplanCore.Response
{
    public class HeavyRefreshResponse
    {
        public string tel { get; set; }
        public string @operator { get; set; }
        public Record_Heavy records { get; set; }
        public int status { get; set; }
        public string desc { get; set; }
    }

    public class Record_Heavy
    {
        public string desc { get; set; }

        public string customerName { get; set; }
    }
}
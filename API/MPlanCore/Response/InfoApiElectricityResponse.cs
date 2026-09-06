namespace MplanCore.Response
{
    public class InfoApiElectricityResponse
    {
        public string status { get; set; }
        public string message { get; set; }
        public string cust { get; set; }
        public int @operator { get; set; }
        public string optional { get; set; }
        public Data3 data { get; set; }
    }

    public class Data3
    {
        public string name { get; set; }
        public bool status { get; set; }
        public int billAmount { get; set; }
        public string billDate { get; set; }
        public string billDueDate { get; set; }
    }
}
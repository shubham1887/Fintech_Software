namespace MplanCore.Response.InfoAPI
{
    public class InfoApiBillResponse
    {
        public string status { get; set; }
        public string message { get; set; }
        public string cust { get; set; }
        public int @operator { get; set; }
        public BillData data { get; set; }
    }

    public class BillData
    {
        public string name { get; set; }
        public bool status { get; set; }
        public int billAmount { get; set; }
        public string billDate { get; set; }
        public string billDueDate { get; set; }
    }

}

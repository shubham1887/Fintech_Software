namespace Project.Response.ClareInfoTech
{
    public class SenderDetail
    {
        public string id { get; set; }
        public string name { get; set; }
        public string mobile { get; set; }
        public string address { get; set; }
        public string pincode { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string kycstatus { get; set; }
        public int consumedlimit { get; set; }
        public int remaininglimit { get; set; }
        public string kycdocs { get; set; }
        public int is_verified { get; set; }
        public int perm_txn_limit { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
    }
}
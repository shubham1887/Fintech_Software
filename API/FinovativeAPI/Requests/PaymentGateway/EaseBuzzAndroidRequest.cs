namespace Project.Requests.PaymentGateway
{
    public class EaseBuzzAndroidRequest
    {
        public string key { get; set; }
        public string txnid { get; set; }
        public string amount { get; set; }

        public string productinfo { get; set; }
        public string firstname { get; set; }
        public string phone { get; set; }

        public string email { get; set; }
        public string surl { get; set; }
        public string furl { get; set; }

        public string hash { get; set; }
        public string udf1 { get; set; }
        public string udf2 { get; set; }

        public string udf3 { get; set; }
        public string udf4 { get; set; }
        public string udf5 { get; set; }
    }
}
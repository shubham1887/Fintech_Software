namespace Project.Response.PaymentGateway
{
    public class EaseBuzzStatusCheckResponse
    {
        public bool status { get; set; }
        public Msg msg { get; set; }
    }

    public class Msg
    {
        public string txnid { get; set; }
        public string firstname { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string key { get; set; }
        public string mode { get; set; }
        public string unmappedstatus { get; set; }
        public string cardCategory { get; set; }
        public string addedon { get; set; }
        public string payment_source { get; set; }
        public string PG_TYPE { get; set; }
        public string bank_ref_num { get; set; }
        public string bankcode { get; set; }
        public string error { get; set; }
        public string error_Message { get; set; }
        public string name_on_card { get; set; }
        public string upi_va { get; set; }
        public string cardnum { get; set; }
        public string issuing_bank { get; set; }
        public string easepayid { get; set; }
        public string amount { get; set; }
        public string net_amount_debit { get; set; }
        public string cash_back_percentage { get; set; }
        public string deduction_percentage { get; set; }
        public string merchant_logo { get; set; }
        public string surl { get; set; }
        public string furl { get; set; }
        public string productinfo { get; set; }
        public string udf10 { get; set; }
        public string udf9 { get; set; }
        public string udf8 { get; set; }
        public string udf7 { get; set; }
        public string udf6 { get; set; }
        public string udf5 { get; set; }
        public string udf4 { get; set; }
        public string udf3 { get; set; }
        public string udf2 { get; set; }
        public string udf1 { get; set; }
        public string card_type { get; set; }
        public string hash { get; set; }
        public string status { get; set; }
        public string bank_name { get; set; }
    }
}
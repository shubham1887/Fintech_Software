using System;

namespace Project.Response.PaymentGateway
{
    public class UPIGatewayResponse
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public int vpa_id { get; set; }
        public string client_vpa { get; set; }
        public int amount { get; set; }

        public string customer_email { get; set; }
        public string customer_mobile { get; set; }
        public string customer_name { get; set; }
        public string customer_vpa { get; set; }
        public string remark { get; set; }
        public string status { get; set; }

        public string redirect_url { get; set; }
        public string client_txn_id { get; set; }
        public string client_name { get; set; }
        public string client_email { get; set; }
        public string client_mobile { get; set; }
        public string p_info { get; set; }
        public string socket_id { get; set; }
        public string upi_txn_id { get; set; }

        public string udf1 { get; set; }
        public string udf2 { get; set; }
        public string udf3 { get; set; }
        public string txnAt { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
}
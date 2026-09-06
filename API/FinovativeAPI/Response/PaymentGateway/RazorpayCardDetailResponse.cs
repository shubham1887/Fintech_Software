namespace Project.Response.PaymentGateway
{
    public class RazorpayCardDetailResponse
    {
        //        {
        //    "id": "card_GslDtF61peOzYO",
        //    "entity": "card",
        //    "name": "vvv",
        //    "last4": "0021",
        //    "network": "Visa",
        //    "type": "debit",
        //    "issuer": "ICIC",
        //    "international": false,
        //    "emi": false,
        //    "sub_type": "consumer"
        //}

        public string id { get; set; }

        public string entity { get; set; }

        public string network { get; set; }
        public string type { get; set; }

        public string sub_type { get; set; }

        public string issuer { get; set; }
    }
}
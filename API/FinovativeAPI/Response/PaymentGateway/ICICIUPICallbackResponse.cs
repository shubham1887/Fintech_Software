namespace Project.Response.PaymentGateway
{
    public class ICICIUPICallbackResponse
    {
        public string TxnStatus { get; set; }
        public string subMerchantId { get; set; }
        public string PayerAmount { get; set; }
        public string PayerName { get; set; }
        public string terminalId { get; set; }
        public string TxnInitDate { get; set; }
        public string merchantTranId { get; set; }
        public string PayerMobile { get; set; }
        public string BankRRN { get; set; }
        public string merchantId { get; set; }
        public string PayerVA { get; set; }
        public string TxnCompletionDate { get; set; }
    }
}
using YESBankCore.Controller;

namespace YesBankCore.Request.BBPS
{
    public class AgentDetailsPay
    {
        public string agentId { get; set; }
        public string partnerId { get; set; }
        public List<DeviceTagPay> deviceTags { get; set; }
    }
    public class DeviceTagPay
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class AmountDetails
    {
        public string amount { get; set; }
        public string currency { get; set; }
        public string custConvFee { get; set; }
        public string couCustConvFee { get; set; }
    }

    public class BillDetailsPay
    {
        public string billerId { get; set; }
        public List<CustomerParamPay> customerParams { get; set; }
    }

    public class CustomerParamPay
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class CustDetailsPay
    {
        public List<CustomerTagPay> customerTags { get; set; }
        public string mobileNo { get; set; }
    }

    public class CustomerTagPay
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class PaymentDetails
    {
        public List<PaymentInfo> paymentInfo { get; set; }
        public string paymentMode { get; set; }
        public string quickPay { get; set; }
        public string splitPay { get; set; }
        public string offusPay { get; set; }
    }

    public class PaymentInfo
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class BbpsPGResponse
    {
        public string txRefNo { get; set; }
        public string txnDateTime { get; set; }
    }
    public class BillPayAPIRequest
    {
        public int chId { get; set; }
        public string refId { get; set; }
        public string clientRequestId { get; set; }
        public AgentDetailsPay agentDetails { get; set; }
        public AmountDetails amountDetails { get; set; }
        public BillDetailsPay billDetails { get; set; }
        public CustDetailsPay custDetails { get; set; }
        public PaymentDetails paymentDetails { get; set; }
        public string bbpsPGName { get; set; }
        public BbpsPGResponse bbpsPGResponse { get; set; }
        
    }
}

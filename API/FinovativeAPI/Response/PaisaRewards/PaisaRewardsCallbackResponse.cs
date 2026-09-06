using System.Collections.Generic;

namespace FinovativeAPI.Response.PaisaRewards
{
    public class PaisaRewardsCallbackResponse
    {
        public List<string> ifscNo { get; set; }
        public List<string> totalAmount { get; set; }
        public List<string> amount { get; set; }
        public List<string> extOrderId { get; set; }
        public List<string> bankRefNo { get; set; }
        public List<string> txnMode { get; set; }
        public List<string> accountNo { get; set; }
        public List<string> responseMessage { get; set; }
        public List<string> transactionId { get; set; }
        public List<object> beneficiaryId { get; set; }
        public List<string> responseCode { get; set; }
    }
}
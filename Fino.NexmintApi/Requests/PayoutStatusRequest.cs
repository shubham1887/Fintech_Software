namespace Fino.NexmintApi.Requests
{
    // Inputs for a Nexmint payout status enquiry (POST /fund-transfer/status). At least one of the
    // two is required; when both are sent, Nexmint resolves on TransactionID.
    public class PayoutStatusRequest
    {
        public string TransactionID { get; set; } = string.Empty; // Nexmint's id, from the initiation reply
        public string OrderID { get; set; } = string.Empty;       // our own ref, submitted at initiation
    }
}

namespace Fino.JaiJuiApi.Requests
{
    // Inputs for a JaiJui transaction status enquiry. AuthCode is resolved from configuration;
    // ID is the APIUserRequestID (our request ref) submitted with the original transfer.
    public class StatusCheckRequest
    {
        public string ID { get; set; } = string.Empty; // RequestID (APIUserRequestID)
    }
}

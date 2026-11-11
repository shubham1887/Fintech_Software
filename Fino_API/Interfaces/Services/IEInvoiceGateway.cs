namespace FintechApp.Interfaces.Services
{
    /// <summary>What the IRP said.</summary>
    public class EInvoiceGatewayResult
    {
        public bool Success { get; set; }
        public string IRN { get; set; } = string.Empty;
        public string AckNo { get; set; } = string.Empty;
        public DateTime? AckDate { get; set; }
        public string SignedQRCode { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string RawResponse { get; set; } = string.Empty;

        /// <summary>
        /// True when the failure is worth another attempt — the portal was down, a timeout, a
        /// throttle. False when it is the document's own fault, because retrying an invalid
        /// payload forever just hides it.
        /// </summary>
        public bool IsRetryable { get; set; }
    }

    /// <summary>
    /// Talks to the Invoice Registration Portal.
    ///
    /// Behind an interface because this platform is resold: each client registers with its own
    /// GSP or uses the NIC portal directly, so the transport differs per install while everything
    /// around it does not. It also means the queue can be exercised without credentials.
    /// </summary>
    public interface IEInvoiceGateway
    {
        /// <summary>Whether this install has an IRP configured at all.</summary>
        bool IsConfigured { get; }

        /// <summary>Register one document. The payload is the IRP's own JSON schema.</summary>
        Task<EInvoiceGatewayResult> Register(string requestPayloadJson);
    }
}

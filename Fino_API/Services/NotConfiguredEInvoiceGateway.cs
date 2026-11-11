using FintechApp.Interfaces.Services;

namespace FintechApp.Services
{
    /// <summary>
    /// The default gateway: there is no IRP configured on this install.
    ///
    /// It fails every registration, loudly and retryably, rather than pretending to succeed. That
    /// is the honest behaviour — a document marked Registered with no IRN would be worse than one
    /// still Pending, because the Pending one shows up on the deadline alert and the fake one does
    /// not. The documents themselves are still valid locally; only their IRP registration is
    /// outstanding.
    ///
    /// Replace this registration in Program.cs with the client's real GSP or NIC implementation.
    /// </summary>
    public class NotConfiguredEInvoiceGateway : IEInvoiceGateway
    {
        public bool IsConfigured => false;

        public Task<EInvoiceGatewayResult> Register(string requestPayloadJson) =>
            Task.FromResult(new EInvoiceGatewayResult
            {
                Success = false,
                ErrorCode = "NOT_CONFIGURED",
                ErrorMessage = "No Invoice Registration Portal is configured on this install, so no IRN can be "
                             + "obtained. At this turnover e-invoicing is mandatory and a notified invoice without "
                             + "a valid IRN is not a valid tax invoice. Configure the GSP or NIC credentials and "
                             + "register the real gateway in Program.cs.",
                // Retryable on purpose: the document is queued and waiting for configuration, not
                // rejected on its own merits.
                IsRetryable = true,
            });
    }
}

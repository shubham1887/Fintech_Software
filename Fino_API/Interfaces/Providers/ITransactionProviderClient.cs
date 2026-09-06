namespace FintechApp.Interfaces.Providers
{
    public class ProviderStatusContext
    {
        public int TransactionMasterID { get; set; }
        public int ProviderID { get; set; }
        public string ProviderRequestID { get; set; } = string.Empty;
        public int ServiceID { get; set; }
        public string ServiceType { get; set; } = string.Empty; // AEPS / BBPS / DMT / RECHARGE / UPI ...
        public int UserID { get; set; }
        public string IPAddress { get; set; } = string.Empty;

        // Admin shift: re-dispatch through this provider instead of the sequence-chosen one.
        // 0 = normal routing. Only meaningful for Resend.
        public int ForceProviderID { get; set; }
    }

    public class ProviderStatusResult
    {
        public string Status { get; set; } = "PENDING"; // SUCCESS / FAILED / PENDING
        public string ProviderRefNo { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string RawResponse { get; set; } = string.Empty;
        public bool IsConfigured { get; set; } = true;
    }

    // Seam for upstream provider status-check / resend calls (DMT, AEPS, BBPS, recharge, UPI, etc.).
    // The pipeline settles the local transaction based on the returned status; only this call is external.
    public interface ITransactionProviderClient
    {
        Task<ProviderStatusResult> CheckStatus(ProviderStatusContext context);
        Task<ProviderStatusResult> Resend(ProviderStatusContext context);
    }
}

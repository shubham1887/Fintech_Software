namespace FintechApp.StaticModels
{
    /// <summary>
    /// The states a complaint can be in. Rows written by older builds used SUCCESS/FAILED for the
    /// decided states, so every read folds the synonyms together rather than comparing raw strings.
    ///
    /// Who moves a complaint where:
    ///   Pending  → Accept / Reject : Admin and Support only (Accept refunds the transaction).
    ///   Pending  → Cancel          : the user who raised it (an admin may cancel on their behalf).
    /// </summary>
    public static class ComplaintStatus
    {
        public const string Pending = "PENDING";
        public const string Accept = "ACCEPT";
        public const string Reject = "REJECT";
        public const string Cancel = "CANCEL";

        public static bool IsAccepted(string? status) =>
            Normalize(status) is "ACCEPT" or "ACCEPTED" or "SUCCESS";

        public static bool IsRejected(string? status) =>
            Normalize(status) is "REJECT" or "REJECTED" or "FAILED" or "FAIL";

        public static bool IsCancelled(string? status) =>
            Normalize(status) is "CANCEL" or "CANCELLED" or "CANCELED";

        /// <summary>Nobody has decided it yet — the only state from which it can still be actioned.</summary>
        public static bool IsOpen(string? status) =>
            !IsAccepted(status) && !IsRejected(status) && !IsCancelled(status);

        private static string Normalize(string? status) => (status ?? string.Empty).Trim().ToUpperInvariant();
    }
}

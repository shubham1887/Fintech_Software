using FintechApp.Entities;

namespace FintechApp.StaticModels
{
    /// <summary>
    /// The one place that decides where a user's KYC stands. Everything else — the admin
    /// approval queue, the user's own profile, the service gate and the user lists — reads its
    /// answer from here, so a user can never be "Pending" on one screen and "Approved" on another.
    ///
    /// The state is DERIVED, never stored twice:
    ///   Approved      → UserMaster.IsKYCApproved (set by the admin, or automatically for E-KYC).
    ///   Rejected      → at least one submitted document was rejected.
    ///   Pending       → something is submitted and waiting, or a required document is missing.
    ///   Not submitted → nothing on file at all.
    /// </summary>
    public static class KycStatus
    {
        public const string Approved = "Approved";
        public const string Pending = "Pending";
        public const string Rejected = "Rejected";
        public const string NotSubmitted = "Not submitted";

        // Document-level statuses stored in UserDocumentMaster.Status.
        public const string DocApproved = "APPROVE";
        public const string DocRejected = "REJECT";
        public const string DocInReview = "PROCESS";

        public static bool IsDocApproved(string? status) =>
            (status ?? string.Empty).Equals(DocApproved, StringComparison.OrdinalIgnoreCase);

        public static bool IsDocRejected(string? status) =>
            (status ?? string.Empty).Equals(DocRejected, StringComparison.OrdinalIgnoreCase);

        public static bool IsDocInReview(string? status) =>
            (status ?? string.Empty).Equals(DocInReview, StringComparison.OrdinalIgnoreCase);

        /// <summary>Where this user's KYC stands, given the documents on file.</summary>
        public static string Resolve(UserMaster user, IEnumerable<UserDocumentMaster> userDocs)
        {
            if (user.IsKYCApproved) return Approved;

            var docs = userDocs.Where(d => !d.IsDeleted).ToList();
            if (docs.Any(d => IsDocRejected(d.Status))) return Rejected;
            if (docs.Count == 0) return NotSubmitted;

            // Something is on file but the user flag is still off: either a document is waiting
            // for review, a required one is missing, or the approval was never finalised. All of
            // those are the same thing to everyone downstream — the admin still has to act.
            return Pending;
        }

        /// <summary>True when every ACTIVE + REQUIRED document is approved for this user.</summary>
        public static bool AllRequiredApproved(IEnumerable<UserDocumentMaster> userDocs, IEnumerable<string> requiredCodes)
        {
            var approved = userDocs
                .Where(d => !d.IsDeleted && IsDocApproved(d.Status))
                .Select(d => (d.DocmentType ?? string.Empty).ToUpperInvariant())
                .ToHashSet();
            var required = requiredCodes.Select(c => (c ?? string.Empty).ToUpperInvariant())
                .Where(c => c.Length > 0).ToList();
            // No required document configured — approval can't be inferred from documents; the
            // admin has to approve the user explicitly.
            if (required.Count == 0) return false;
            return required.All(approved.Contains);
        }
    }
}

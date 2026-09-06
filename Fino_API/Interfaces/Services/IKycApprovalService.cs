using FintechApp.Requests.Kyc;
using FintechApp.Responses.Kyc;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// The admin side of KYC: the review queue, the approve / reject decision, and the policy that
    /// says what a user may still do while their KYC is pending. Read-only for support, write for
    /// the administrator (enforced at the controller).
    /// </summary>
    public interface IKycApprovalService
    {
        Task<KycUserListResponse> SelectUsers(KycUserListRequest request);

        Task<(string failureMessage, KycUserDetail? data)> SelectUser(int userID);

        /// <summary>Approve or reject one submitted document. Approving the last outstanding
        /// requirement approves the user's KYC as a whole.</summary>
        Task<string> ReviewDocument(KycReviewDocumentRequest request, int adminUserID);

        /// <summary>Approve or reject the user's KYC in one action.</summary>
        Task<string> ReviewUser(KycReviewUserRequest request, int adminUserID);

        Task<KycPolicyResponse> SelectPolicy();

        Task<string> SavePolicy(KycPolicySaveRequest request, int adminUserID);

        /// <summary>
        /// Approve the KYC of a user who completed E-KYC, without an admin ever touching it. Called
        /// from the E-KYC flow; safe to call repeatedly.
        /// </summary>
        Task<string> AutoApproveEkyc(int userID);
    }
}

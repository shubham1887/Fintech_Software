using FintechApp.Requests.User;
using FintechApp.Responses.User;

namespace FintechApp.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserResponse?> GetUserById(int userId);
        Task<List<UserResponse>> GetUsers(GetUserRequest request);
        Task<bool> UpdateUser(int userId, string fieldName, string oldValue, string newValue);

        // UsersController critical methods
        Task<string> Insert(UserInsertRequest request, int currentUserID, string ipAddress);
        Task<string> Update(UserUpdateRequest request, int currentUserID);
        Task<string> Activate(UserActivateRequest request, int currentUserID);
        Task<string> Deactivate(int targetUserID, int currentUserID);
        Task<string> Delete(UserDeleteRequest request, int currentUserID);
        Task<string> Restore(UserDeleteRequest request, int currentUserID);
        Task<string> CommissionHoldActivate(UserCommissionHoldRequest request, int currentUserID);
        Task<string> IsAppActivate(UserBooleanToggleRequest request, int currentUserID);
        Task<string> IsWebActivate(UserBooleanToggleRequest request, int currentUserID);
        Task<string> IsInvoiceActivate(UserBooleanToggleRequest request, int currentUserID);
        // Returns (failureMessage, summary): the summary carries "N updated, M skipped and why"
        // so a partly-applied batch reads as the success it is rather than as an error.
        Task<(string failureMessage, string summary)> BulkUpdate(UserBulkUpdateRequest request, int currentUserID);
        Task<string> SendPassword(UserSendPasswordRequest request, int currentUserID);
        Task<string> AddHoldBalance(UserAddHoldBalanceRequest request, int currentUserID);

        // The hold report — every hold placed or released, scoped to what the caller may see.
        Task<(string failureMessage, Responses.User.WalletHoldHistoryWrapper data)> HoldHistory(
            WalletHoldHistoryRequest request, int callerUserID, int callerUserTypeID);

        // Move one user (and their branch) under a new parent. Returns (failure, summary) so the
        // reply can name who moved where, and how many travelled with them.
        Task<(string failureMessage, string summary)> TransferUsers(UserTransferRequest request, int currentUserID);

        // Who may legally become a given user's parent — drives the "New parent" picker so the
        // User Transfer screen cannot offer a move the tier rules would refuse.
        Task<(string failureMessage, List<UserDDLResponse> data)> SelectEligibleParents(
            UserSelectByIDRequest request, int currentUserID);

        // The user change report — every change made to a user record, with who, why and from where.
        Task<(string failureMessage, Responses.User.UserChangeHistoryWrapper data)> ChangeHistory(
            UserChangeHistoryRequest request, int callerUserID, int callerUserTypeID);
        Task<string> Deactivate2FAWithOTP(UserDeactivate2FARequest request, int currentUserID);
        Task<(string failureMessage, UserDetailViewModel? data)> SelectByID(UserSelectByIDRequest request, int currentUserID);

        // ── Self-service profile: the caller's OWN record, never anybody else's ─────
        Task<(string failureMessage, UserDetailViewModel? data)> SelectProfile(int currentUserID);
        Task<string> UpdateProfile(UserProfileUpdateRequest request, int currentUserID);

        // One mobile number, two roles — see StaticModels/LinkedProfilePolicy.
        Task<(string failureMessage, LinkedProfilesResponse data)> GetLinkedProfiles(int currentUserID);
        Task<(string failureMessage, LinkedProfilesResponse data)> CreateLinkedProfile(CreateLinkedProfileRequest request, int currentUserID);
        Task<(string failureMessage, UserListWrapper data)> Select(UserSelectRequest request, int currentUserID, int currentUserTypeID = 0);
        Task<(string failureMessage, List<UserDDLResponse> data)> SelectForDDL(UserDDLRequest request, int currentUserID);
        Task<(string failureMessage, List<UserDDLResponse> data)> SelectForDDLWithUserType(UserDDLRequest request, int currentUserID);
        Task<(string failureMessage, UserGSTViewModel? data)> GSTView(UserGSTRequest request);
        Task<(string failureMessage, PincodeDetailsResponse? data)> FetchDetailsForPincode(string pincode);
        Task<(string failureMessage, string token)> LoginAsUser(UserLoginAsRequest request, int currentUserID);

        // ── API-user self-service: API token + IP security (OTP-gated) ──────────────
        Task<(string failureMessage, APITokenResponse? data)> SelectAPIToken(int currentUserID);
        Task<string> SendAPITokenOTP(int currentUserID);
        Task<(string failureMessage, APITokenResponse? data)> TokenGeneration(TokenGenerationRequest request, int currentUserID);
        Task<(string failureMessage, SecureIPResponse? data)> SelectIsSecureIP(int currentUserID);
        Task<string> SendSecureIPOTP(int currentUserID);
        Task<string> UpdateIsSecureIP(UpdateSecureIPRequest request, int currentUserID);
    }
}

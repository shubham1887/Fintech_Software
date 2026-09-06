using FintechApp.Requests.Beneficiary;
using FintechApp.Responses.Beneficiary;

namespace FintechApp.Interfaces.Services
{
    public interface IDMTBeneficiaryFlowService
    {
        Task<(string failureMessage, AddBeneficiaryResponse? response)> AddBeneficiary(AddBeneficiaryRequest request, int userId, string ipAddress);
        Task<string> VerifyOTP(VerifyBeneficiaryOTPRequest request, int userId);
        Task<string> ResendOTP(ResendBeneficiaryOTPRequest request, int userId);
        Task<(string failureMessage, List<BeneficiaryDetailResponse> response)> GetBySender(SelectBeneficiaryRequest request);
        Task<(string failureMessage, BeneficiaryDetailResponse? response)> GetByGUID(SelectBeneficiaryByGUIDRequest request);
        Task<string> Delete(DeleteBeneficiaryByGUIDRequest request);
        Task<(string failureMessage, CheckBranchResponse response)> CheckBranch(CheckBranchRequest request);
    }
}

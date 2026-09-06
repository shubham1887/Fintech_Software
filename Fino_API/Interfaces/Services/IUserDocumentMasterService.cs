using FintechApp.Requests.KYC;
using FintechApp.Requests.UserDocumentMaster;
using FintechApp.Responses.KYC;
using FintechApp.Responses.UserDocumentMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IUserDocumentMasterService
    {
        Task<List<UserDocumentMasterResponse>> GetAll();
        Task<UserDocumentMasterResponse?> GetByID(int id);
        Task<List<UserDocumentMasterResponse>> GetByUserID(int userID);
        Task<string> Create(CreateUserDocumentMasterRequest request);
        Task<string> Update(UpdateUserDocumentMasterRequest request);
        Task<string> Delete(int id);

        Task<(string failureMessage, string result)> UploadKYC(UploadKYCRequest request, int userID);
        Task<string> ApproveKYC(ApproveKYCRequest request, int adminUserID);
        Task<(string failureMessage, KYCWrapper? data)> SelectKYC(SelectKYCRequest request);
        Task<(string failureMessage, List<KYCViewModel> data)> SelectPendingKYC();
        Task<(string failureMessage, List<KYCViewModel> data)> SelectDocsByUserID(int userID);
        Task<(string failureMessage, KYCViewModel? data)> SelectKYCByID(int userDocumentID);
        Task<(string failureMessage, List<KYCActiveDocViewModel> data)> SelectActiveDocUserwise(int userID);
    }
}

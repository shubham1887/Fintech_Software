using FintechApp.Requests.UserMerchantMaster;
using FintechApp.Responses.UserMerchantMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IUserMerchantMasterService
    {
        Task<List<UserMerchantMasterResponse>> GetAll();
        Task<UserMerchantMasterResponse?> GetByID(int id);
        Task<List<UserMerchantMasterResponse>> GetByUserID(int userid);
        Task<string> Create(CreateUserMerchantMasterRequest request);
        Task<string> Update(UpdateUserMerchantMasterRequest request);
        Task<string> Delete(int id);
    }
}

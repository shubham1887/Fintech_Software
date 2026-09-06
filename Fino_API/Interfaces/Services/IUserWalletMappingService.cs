using FintechApp.Requests.UserWalletMapping;
using FintechApp.Responses.UserWalletMapping;
namespace FintechApp.Interfaces.Services
{
    public interface IUserWalletMappingService
    {
        Task<List<UserWalletMappingResponse>> GetAll();
        Task<List<UserWalletMappingResponse>> GetByUserID(int userId);
        Task<UserWalletMappingResponse?> GetByID(int id);
        Task<string> Create(CreateUserWalletMappingRequest request);
        Task<string> Update(UpdateUserWalletMappingRequest request);
        Task<string> Delete(int id);
    }
}

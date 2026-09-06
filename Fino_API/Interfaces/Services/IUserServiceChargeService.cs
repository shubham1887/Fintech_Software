using FintechApp.Requests.UserServiceCharge;
using FintechApp.Responses.UserServiceCharge;
namespace FintechApp.Interfaces.Services
{
    public interface IUserServiceChargeService
    {
        Task<List<UserServiceChargeResponse>> GetAll();
        Task<List<UserServiceChargeResponse>> GetByUserID(int userId);
        Task<UserServiceChargeResponse?> GetByID(int id);
        Task<string> Create(CreateUserServiceChargeRequest request);
        Task<string> Update(UpdateUserServiceChargeRequest request);
        Task<string> Delete(int id);
    }
}

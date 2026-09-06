using FintechApp.Requests.UserCreationSetting;
using FintechApp.Responses.UserCreationSetting;

namespace FintechApp.Interfaces.Services
{
    public interface IUserCreationSettingService
    {
        Task<List<UserCreationSettingResponse>> GetAll();
        Task<UserCreationSettingResponse?> GetByID(int id);
        Task<List<UserCreationSettingResponse>> GetByUserID(int userID);
        Task<string> Create(CreateUserCreationSettingRequest request);
        Task<string> Update(UpdateUserCreationSettingRequest request);
        Task<string> Delete(int id);
    }
}

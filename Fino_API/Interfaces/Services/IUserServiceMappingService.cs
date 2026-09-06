using FintechApp.Requests.UserServiceMapping;
using FintechApp.Responses.UserServiceMapping;
namespace FintechApp.Interfaces.Services
{
    public interface IUserServiceMappingService
    {
        Task<List<UserServiceMappingResponse>> GetAll();
        Task<List<UserServiceMappingResponse>> GetByUserID(int userId);
        Task<UserServiceMappingResponse?> GetByID(int id);
        Task<string> Create(CreateUserServiceMappingRequest request);
        Task<string> Update(UpdateUserServiceMappingRequest request);
        Task<string> Delete(int id);
    }
}

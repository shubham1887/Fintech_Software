using FintechApp.Requests.UserSlabMapping;
using FintechApp.Responses.UserSlabMapping;

namespace FintechApp.Interfaces.Services
{
    public interface IUserSlabMappingService
    {
        Task<List<UserSlabMappingResponse>> GetAll();
        Task<List<UserSlabMappingResponse>> GetByUserID(int userId);
        Task<UserSlabMappingResponse?> GetByID(int userSlabMappingId);
        Task<string> Create(CreateUserSlabMappingRequest request);
        Task<string> Update(UpdateUserSlabMappingRequest request);
        Task<string> Delete(int userSlabMappingId);
    }
}

using FintechApp.Requests.UserChannelSlabMapping;
using FintechApp.Responses.UserChannelSlabMapping;

namespace FintechApp.Interfaces.Services
{
    public interface IUserChannelSlabMappingService
    {
        Task<List<UserChannelSlabMappingResponse>> GetAll();
        Task<List<UserChannelSlabMappingResponse>> GetByUserID(int userId);
        Task<UserChannelSlabMappingResponse?> GetByID(int userChannelSlabMappingId);
        Task<string> Create(CreateUserChannelSlabMappingRequest request);
        Task<string> Update(UpdateUserChannelSlabMappingRequest request);
        Task<string> Delete(int userChannelSlabMappingId);
    }
}

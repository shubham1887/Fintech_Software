using FintechApp.Requests.ProviderUserMapping;
using FintechApp.Responses.ProviderUserMapping;
namespace FintechApp.Interfaces.Services
{
    public interface IProviderUserMappingService
    {
        Task<List<ProviderUserMappingResponse>> GetAll();
        Task<List<ProviderUserMappingResponse>> GetByUserID(int userId);
        Task<ProviderUserMappingResponse?> GetByID(int id);
        Task<string> Create(CreateProviderUserMappingRequest request);
        Task<string> Update(UpdateProviderUserMappingRequest request);
        Task<string> Delete(int id);
    }
}

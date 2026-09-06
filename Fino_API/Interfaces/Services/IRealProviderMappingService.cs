using FintechApp.Requests.RealProviderMapping;
using FintechApp.Responses.RealProviderMapping;
namespace FintechApp.Interfaces.Services
{
    public interface IRealProviderMappingService
    {
        Task<List<RealProviderMappingResponse>> GetAll();
        Task<RealProviderMappingResponse?> GetByID(int id);
        Task<string> Create(CreateRealProviderMappingRequest request);
        Task<string> Update(UpdateRealProviderMappingRequest request);
        Task<string> Delete(int id);
    }
}

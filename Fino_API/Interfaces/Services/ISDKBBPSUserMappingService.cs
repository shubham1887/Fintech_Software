using FintechApp.Requests.SDKBBPSUserMapping;
using FintechApp.Responses.SDKBBPSUserMapping;
namespace FintechApp.Interfaces.Services
{
    public interface ISDKBBPSUserMappingService
    {
        Task<List<SDKBBPSUserMappingResponse>> GetAll();
        Task<SDKBBPSUserMappingResponse?> GetByID(int id);
        Task<List<SDKBBPSUserMappingResponse>> GetByUserID(int userid);
        Task<string> Create(CreateSDKBBPSUserMappingRequest request);
        Task<string> Update(UpdateSDKBBPSUserMappingRequest request);
        Task<string> Delete(int id);
    }
}

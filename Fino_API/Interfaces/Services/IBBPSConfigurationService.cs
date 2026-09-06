using FintechApp.Entities;
using FintechApp.Requests.BBPSConfiguration;
using FintechApp.Responses.BBPSConfiguration;

namespace FintechApp.Interfaces.Services
{
    public interface IBBPSConfigurationService
    {
        Task<BBPSConfigurationResponse?> GetBBPSConfigurationById(int bbpsConfigurationID);
        Task<List<BBPSConfigurationResponse>> GetBBPSConfigurations(GetBBPSConfigurationRequest request);
        Task<string> CreateBBPSConfiguration(CreateBBPSConfigurationRequest request);
        Task<string> UpdateBBPSConfiguration(UpdateBBPSConfigurationRequest request);
        Task<string> DeleteBBPSConfiguration(DeleteBBPSConfigurationRequest request);
    }
}
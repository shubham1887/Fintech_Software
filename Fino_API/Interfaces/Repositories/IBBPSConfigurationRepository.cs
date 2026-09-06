using FintechApp.Entities;
using FintechApp.Requests.BBPSConfiguration;

namespace FintechApp.Interfaces.Repositories
{
    public interface IBBPSConfigurationRepository
    {
        Task<List<BBPSConfiguration>> GetAllBBPSConfigurations();
        Task<BBPSConfiguration?> GetBBPSConfigurationById(int bbpsConfigurationID);
        Task<List<BBPSConfiguration>> GetBBPSConfigurations(GetBBPSConfigurationRequest request);
        Task<bool> CreateBBPSConfiguration(BBPSConfiguration bbpsConfiguration);
        Task<bool> UpdateBBPSConfiguration(BBPSConfiguration bbpsConfiguration);
        Task<bool> DeleteBBPSConfiguration(BBPSConfiguration bbpsConfiguration);

    }
}
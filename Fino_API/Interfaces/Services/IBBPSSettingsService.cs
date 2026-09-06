using FintechApp.Requests.BBPSSettings;
using FintechApp.Responses.BBPSSettings;
namespace FintechApp.Interfaces.Services
{
    public interface IBBPSSettingsService
    {
        Task<List<BBPSSettingsResponse>> GetAll();
        Task<BBPSSettingsResponse?> GetByID(int id);
        Task<string> Create(CreateBBPSSettingsRequest request);
        Task<string> Update(UpdateBBPSSettingsRequest request);
        Task<string> Delete(int id);
    }
}

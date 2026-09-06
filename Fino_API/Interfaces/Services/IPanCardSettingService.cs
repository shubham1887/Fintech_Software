using FintechApp.Requests.PanCardSetting;
using FintechApp.Responses.PanCardSetting;
namespace FintechApp.Interfaces.Services
{
    public interface IPanCardSettingService
    {
        Task<List<PanCardSettingResponse>> GetAll();
        Task<PanCardSettingResponse?> GetByID(int id);

        Task<string> Create(CreatePanCardSettingRequest request);
        Task<string> Update(UpdatePanCardSettingRequest request);
        Task<string> Delete(int id);
    }
}

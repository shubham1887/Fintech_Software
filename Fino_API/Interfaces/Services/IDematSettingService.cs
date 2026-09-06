using FintechApp.Requests.DematSetting;
using FintechApp.Responses.DematSetting;
namespace FintechApp.Interfaces.Services
{
    public interface IDematSettingService
    {
        Task<List<DematSettingResponse>> GetAll();
        Task<DematSettingResponse?> GetByID(int id);

        Task<string> Create(CreateDematSettingRequest request);
        Task<string> Update(UpdateDematSettingRequest request);
        Task<string> Delete(int id);
    }
}

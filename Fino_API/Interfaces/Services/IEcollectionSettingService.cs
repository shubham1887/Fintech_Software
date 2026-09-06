using FintechApp.Requests.EcollectionSetting;
using FintechApp.Responses.EcollectionSetting;
namespace FintechApp.Interfaces.Services
{
    public interface IEcollectionSettingService
    {
        Task<List<EcollectionSettingResponse>> GetAll();
        Task<EcollectionSettingResponse?> GetByID(int id);
        Task<string> Create(CreateEcollectionSettingRequest request);
        Task<string> Update(UpdateEcollectionSettingRequest request);
        Task<string> Delete(int id);
    }
}

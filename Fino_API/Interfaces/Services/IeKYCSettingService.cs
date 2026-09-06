using FintechApp.Requests.eKYCSetting;
using FintechApp.Responses.eKYCSetting;
namespace FintechApp.Interfaces.Services
{
    public interface IeKYCSettingService
    {
        Task<List<eKYCSettingResponse>> GetAll();
        Task<eKYCSettingResponse?> GetByID(int id);

        Task<string> Create(CreateeKYCSettingRequest request);
        Task<string> Update(UpdateeKYCSettingRequest request);
        Task<string> Delete(int id);
    }
}

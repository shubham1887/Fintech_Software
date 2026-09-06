using FintechApp.Requests.ServiceWiseSliderSetting;
using FintechApp.Responses.ServiceWiseSliderSetting;
namespace FintechApp.Interfaces.Services
{
    public interface IServiceWiseSliderSettingService
    {
        Task<List<ServiceWiseSliderSettingResponse>> GetAll();
        Task<ServiceWiseSliderSettingResponse?> GetByID(int id);
        Task<List<ServiceWiseSliderSettingResponse>> GetByServiceID(int serviceID);
        Task<string> Create(CreateServiceWiseSliderSettingRequest request);
        Task<string> Update(UpdateServiceWiseSliderSettingRequest request);
        Task<string> Delete(int id);
    }
}

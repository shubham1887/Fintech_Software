using FintechApp.Requests.DeviceWiseFailLoginHistory;
using FintechApp.Responses.DeviceWiseFailLoginHistory;
namespace FintechApp.Interfaces.Services
{
    public interface IDeviceWiseFailLoginHistoryService
    {
        Task<List<DeviceWiseFailLoginHistoryResponse>> GetAll();
        Task<DeviceWiseFailLoginHistoryResponse?> GetByID(int id);

        Task<string> Create(CreateDeviceWiseFailLoginHistoryRequest request);
        Task<string> Update(UpdateDeviceWiseFailLoginHistoryRequest request);
        Task<string> Delete(int id);
    }
}

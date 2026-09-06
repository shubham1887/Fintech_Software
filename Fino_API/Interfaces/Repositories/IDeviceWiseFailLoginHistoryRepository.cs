using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IDeviceWiseFailLoginHistoryRepository
    {
        Task<List<DeviceWiseFailLoginHistory>> GetAll();
        Task<DeviceWiseFailLoginHistory?> GetByID(int id);

        Task<int> Insert(DeviceWiseFailLoginHistory entity);
        Task<int> Update(DeviceWiseFailLoginHistory entity);
        Task<int> Delete(int id);
        Task<int> GetTodayFailCount(string userName, string deviceId);
    }
}

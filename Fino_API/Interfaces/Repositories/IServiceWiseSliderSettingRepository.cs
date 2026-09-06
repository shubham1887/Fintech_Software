using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IServiceWiseSliderSettingRepository
    {
        Task<List<ServiceWiseSliderSetting>> GetAll();
        Task<ServiceWiseSliderSetting?> GetByID(int id);
        Task<List<ServiceWiseSliderSetting>> GetByServiceID(int serviceID);
        Task<int> Insert(ServiceWiseSliderSetting entity);
        Task<int> Update(ServiceWiseSliderSetting entity);
        Task<int> Delete(int id);
    }
}

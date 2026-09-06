using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IeKYCSettingRepository
    {
        Task<List<eKYCSetting>> GetAll();
        Task<eKYCSetting?> GetByID(int id);

        Task<int> Insert(eKYCSetting entity);
        Task<int> Update(eKYCSetting entity);
        Task<int> Delete(int id);
    }
}

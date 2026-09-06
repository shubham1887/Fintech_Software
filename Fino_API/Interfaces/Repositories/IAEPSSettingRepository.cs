using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IAEPSSettingRepository
    {
        Task<List<AEPSSetting>> GetAll();
        Task<AEPSSetting?> GetByID(int id);
        Task<AEPSSetting?> GetByName(string name);
        Task<int> Insert(AEPSSetting entity);
        Task<int> Update(AEPSSetting entity);
        Task<int> Delete(int id);
    }
}

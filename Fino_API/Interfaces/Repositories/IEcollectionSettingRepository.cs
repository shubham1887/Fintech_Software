using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IEcollectionSettingRepository
    {
        Task<List<EcollectionSetting>> GetAll();
        Task<EcollectionSetting?> GetByID(int id);
        Task<int> Insert(EcollectionSetting entity);
        Task<int> Update(EcollectionSetting entity);
        Task<int> Delete(int id);
    }
}

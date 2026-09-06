using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IMicroATMSettingRepository
    {
        Task<List<MicroATMSetting>> GetAll();
        Task<MicroATMSetting?> GetByID(int id);

        Task<int> Insert(MicroATMSetting entity);
        Task<int> Update(MicroATMSetting entity);
        Task<int> Delete(int id);
    }
}

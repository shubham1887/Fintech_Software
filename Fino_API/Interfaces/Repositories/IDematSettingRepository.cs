using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IDematSettingRepository
    {
        Task<List<DematSetting>> GetAll();
        Task<DematSetting?> GetByID(int id);

        Task<int> Insert(DematSetting entity);
        Task<int> Update(DematSetting entity);
        Task<int> Delete(int id);
    }
}

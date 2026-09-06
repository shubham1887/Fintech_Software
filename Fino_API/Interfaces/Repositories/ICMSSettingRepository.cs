using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ICMSSettingRepository
    {
        Task<List<CMSSetting>> GetAll();
        Task<CMSSetting?> GetByID(int id);

        Task<int> Insert(CMSSetting entity);
        Task<int> Update(CMSSetting entity);
        Task<int> Delete(int id);
    }
}

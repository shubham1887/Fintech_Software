using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IAndroidMenuMasterRepository
    {
        Task<List<AndroidMenuMaster>> GetAll();
        Task<AndroidMenuMaster?> GetByID(int id);

        Task<int> Insert(AndroidMenuMaster entity);
        Task<int> Update(AndroidMenuMaster entity);
        Task<int> Delete(int id);
    }
}

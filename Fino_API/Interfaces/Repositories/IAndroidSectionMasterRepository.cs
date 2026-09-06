using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IAndroidSectionMasterRepository
    {
        Task<List<AndroidSectionMaster>> GetAll();
        Task<AndroidSectionMaster?> GetByID(int id);

        Task<int> Insert(AndroidSectionMaster entity);
        Task<int> Update(AndroidSectionMaster entity);
        Task<int> Delete(int id);
    }
}

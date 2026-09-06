using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IOverdraftMasterRepository
    {
        Task<List<OverdraftMaster>> GetAll();
        Task<OverdraftMaster?> GetByID(int id);
        Task<List<OverdraftMaster>> GetByUserID(int userid);
        Task<int> Insert(OverdraftMaster entity);
        Task<int> Update(OverdraftMaster entity);
        Task<int> Delete(int id);
    }
}

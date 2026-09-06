using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserActionMasterRepository
    {
        Task<List<UserActionMaster>> GetAll();
        Task<UserActionMaster?> GetByID(int id);
        Task<List<UserActionMaster>> GetByUserID(int userid);
        Task<int> Insert(UserActionMaster entity);
        Task<int> Update(UserActionMaster entity);
        Task<int> Delete(int id);
    }
}

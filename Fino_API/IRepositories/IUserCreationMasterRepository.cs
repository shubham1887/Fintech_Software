using FintechApp.Entities;

namespace FintechApp.IRepositories
{
    public interface IUserCreationMasterRepository
    {
        Task<List<UserCreationMaster>> GetAll();
        Task<UserCreationMaster?> GetByID(int id);
        Task<List<UserCreationMaster>> GetByUserID(int userID);
        Task<int> Insert(UserCreationMaster entity);
        Task<int> Update(UserCreationMaster entity);
        Task<int> Delete(int id);
    }
}

using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IUserTypeMasterRepository
    {
        Task<List<UserTypeMaster>> GetAll();
        Task<UserTypeMaster?> GetByID(int userTypeId);
        Task<UserTypeMaster?> GetByName(string userType);
        Task<int> Insert(UserTypeMaster entity);
        Task<int> Update(UserTypeMaster entity);
        Task<int> Delete(int userTypeId);
    }
}

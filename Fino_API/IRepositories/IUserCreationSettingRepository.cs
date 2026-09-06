using FintechApp.Entities;

namespace FintechApp.IRepositories
{
    public interface IUserCreationSettingRepository
    {
        Task<List<UserCreationSetting>> GetAll();
        Task<UserCreationSetting?> GetByID(int id);
        Task<List<UserCreationSetting>> GetByUserID(int userID);
        Task<int> Insert(UserCreationSetting entity);
        Task<int> Update(UserCreationSetting entity);
        Task<int> Delete(int id);
    }
}

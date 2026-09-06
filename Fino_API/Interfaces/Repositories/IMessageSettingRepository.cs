using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IMessageSettingRepository
    {
        Task<List<MessageSetting>> GetAll();
        Task<MessageSetting?> GetByID(int messageSettingId);
        Task<MessageSetting?> GetByType(string type);
        Task<int> Insert(MessageSetting entity);
        Task<int> Update(MessageSetting entity);
        Task<int> Delete(int messageSettingId);
    }
}

using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IDMTSenderRepository
    {
        Task<List<DMTSender>> GetAll();
        Task<DMTSender?> GetByID(int id);
        Task<DMTSender?> GetByMobile(string mobile);
        Task<List<DMTSender>> GetByUserID(int userId);
        Task<int> Insert(DMTSender entity);
        Task<int> Update(DMTSender entity);
        Task<int> Delete(int id);
    }
}

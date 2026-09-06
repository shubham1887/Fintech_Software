using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ISentSMSRepository
    {
        Task<List<SentSMS>> GetAll();
        Task<SentSMS?> GetByID(int id);
        Task<List<SentSMS>> GetByUserID(int userID);
        Task<int> Insert(SentSMS entity);
        Task<int> Update(SentSMS entity);
        Task<int> Delete(int id);
    }
}

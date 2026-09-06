using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IInboxRepository
    {
        Task<List<Inbox>> GetAll();
        Task<Inbox?> GetByID(int id);
        Task<List<Inbox>> GetByUserID(int userID);
        Task<int> Insert(Inbox entity);
        Task<int> Update(Inbox entity);
        Task<int> Delete(int id);
    }
}

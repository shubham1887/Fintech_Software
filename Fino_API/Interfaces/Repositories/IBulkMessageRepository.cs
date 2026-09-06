using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IBulkMessageRepository
    {
        Task<List<BulkMessage>> GetAll();
        Task<BulkMessage?> GetByID(int id);
        Task<List<BulkMessage>> GetByUserID(int userID);
        Task<int> Insert(BulkMessage entity);
        Task<int> Update(BulkMessage entity);
        Task<int> Delete(int id);
    }
}

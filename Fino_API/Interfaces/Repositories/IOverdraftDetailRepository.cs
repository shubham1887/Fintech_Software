using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IOverdraftDetailRepository
    {
        Task<List<OverdraftDetail>> GetAll();
        Task<OverdraftDetail?> GetByID(int id);
        Task<List<OverdraftDetail>> GetByUserID(int userid);
        Task<int> Insert(OverdraftDetail entity);
        Task<int> Update(OverdraftDetail entity);
        Task<int> Delete(int id);
    }
}

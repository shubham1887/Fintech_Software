using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ISenderRepository
    {
        Task<List<Sender>> GetAll();
        Task<Sender?> GetByID(int id);
        Task<List<Sender>> GetByUserID(int userid);
        Task<int> Insert(Sender entity);
        Task<int> Update(Sender entity);
        Task<int> Delete(int id);
    }
}

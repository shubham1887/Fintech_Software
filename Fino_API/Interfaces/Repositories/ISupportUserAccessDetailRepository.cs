using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ISupportUserAccessDetailRepository
    {
        Task<List<SupportUserAccessDetail>> GetAll();
        Task<SupportUserAccessDetail?> GetByID(int id);
        Task<List<SupportUserAccessDetail>> GetByUserID(int userid);
        Task<int> Insert(SupportUserAccessDetail entity);
        Task<int> Update(SupportUserAccessDetail entity);
        Task<int> Delete(int id);
    }
}

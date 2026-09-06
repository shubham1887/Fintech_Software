using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface INewsMasterRepository
    {
        Task<List<NewsMaster>> GetAll();
        Task<NewsMaster?> GetByID(int newsId);
        Task<int> Insert(NewsMaster entity);
        Task<int> Update(NewsMaster entity);
        Task<int> Delete(int newsId);
    }
}

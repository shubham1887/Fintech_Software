using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IExternalMediaRepository
    {
        Task<List<ExternalMedia>> GetAll();
        Task<ExternalMedia?> GetByID(int id);
        Task<int> Insert(ExternalMedia entity);
        Task<int> Update(ExternalMedia entity);
        Task<int> Delete(int id);
    }
}

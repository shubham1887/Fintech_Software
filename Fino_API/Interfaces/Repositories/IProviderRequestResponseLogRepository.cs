using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IProviderRequestResponseLogRepository
    {
        Task<List<ProviderRequestResponseLog>> GetAll();
        Task<ProviderRequestResponseLog?> GetByID(int id);
        Task<List<ProviderRequestResponseLog>> GetByUserID(int userID);
        Task<int> Insert(ProviderRequestResponseLog entity);
        Task<int> Update(ProviderRequestResponseLog entity);
        Task<int> Delete(int id);
    }
}

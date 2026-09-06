using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ITrustableDeviceRepository
    {
        Task<List<TrustableDevice>> GetAll();
        Task<TrustableDevice?> GetByID(int id);
        Task<List<TrustableDevice>> GetByUserID(int userid);
        Task<int> Insert(TrustableDevice entity);
        Task<int> Update(TrustableDevice entity);
        Task<int> Delete(int id);
        Task<TrustableDevice?> GetByUserAndBrowser(int userId, string browser);
    }
}

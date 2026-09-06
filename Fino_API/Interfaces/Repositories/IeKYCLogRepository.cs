using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IeKYCLogRepository
    {
        Task<List<eKYCLog>> GetAll();
        Task<eKYCLog?> GetByID(int id);
        Task<List<eKYCLog>> GetByUserID(int userid);
        Task<int> Insert(eKYCLog entity);
        Task<int> Update(eKYCLog entity);
        Task<int> Delete(int id);
    }
}

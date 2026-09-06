using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IAPIUserCallbackRequestResponseLogRepository
    {
        Task<List<APIUserCallbackRequestResponseLog>> GetAll();
        Task<APIUserCallbackRequestResponseLog?> GetByID(int id);
        Task<List<APIUserCallbackRequestResponseLog>> GetByUserID(int userID);
        Task<int> Insert(APIUserCallbackRequestResponseLog entity);
        Task<int> Update(APIUserCallbackRequestResponseLog entity);
        Task<int> Delete(int id);
    }
}

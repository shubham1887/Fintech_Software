using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IAPIUserInitialRequestResponseLogRepository
    {
        Task<List<APIUserInitialRequestResponseLog>> GetAll();
        Task<APIUserInitialRequestResponseLog?> GetByID(int id);
        Task<List<APIUserInitialRequestResponseLog>> GetByUserID(int userID);
        Task<int> Insert(APIUserInitialRequestResponseLog entity);
        Task<int> Update(APIUserInitialRequestResponseLog entity);
        Task<int> Delete(int id);
    }
}

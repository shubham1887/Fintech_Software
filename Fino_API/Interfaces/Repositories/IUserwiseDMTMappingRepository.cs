using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserwiseDMTMappingRepository
    {
        Task<List<UserwiseDMTMapping>> GetAll();
        Task<List<UserwiseDMTMapping>> GetByUserID(int userId);
        Task<UserwiseDMTMapping?> GetByID(int id);
        Task<int> Insert(UserwiseDMTMapping entity);
        Task<int> Update(UserwiseDMTMapping entity);
        Task<int> Delete(int id);
    }
}

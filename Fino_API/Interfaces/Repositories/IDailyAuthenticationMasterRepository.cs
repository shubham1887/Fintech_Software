using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IDailyAuthenticationMasterRepository
    {
        Task<List<DailyAuthenticationMaster>> GetAll();
        Task<DailyAuthenticationMaster?> GetByID(int id);
        Task<List<DailyAuthenticationMaster>> GetByUserID(int userid);
        Task<int> Insert(DailyAuthenticationMaster entity);
        Task<int> Update(DailyAuthenticationMaster entity);
        Task<int> Delete(int id);
    }
}

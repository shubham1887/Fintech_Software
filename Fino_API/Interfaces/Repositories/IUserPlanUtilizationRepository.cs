using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserPlanUtilizationRepository
    {
        Task<List<UserPlanUtilization>> GetAll();
        Task<UserPlanUtilization?> GetByID(int id);
        Task<List<UserPlanUtilization>> GetByUserID(int userid);
        Task<int> Insert(UserPlanUtilization entity);
        Task<int> Update(UserPlanUtilization entity);
        Task<int> Delete(int id);
    }
}

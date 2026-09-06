using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserWisePlanLimitRepository
    {
        Task<List<UserWisePlanLimit>> GetAll();
        Task<UserWisePlanLimit?> GetByID(int id);
        Task<List<UserWisePlanLimit>> GetByUserID(int userid);
        Task<int> Insert(UserWisePlanLimit entity);
        Task<int> Update(UserWisePlanLimit entity);
        Task<int> Delete(int id);
    }
}

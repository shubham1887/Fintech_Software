using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserWisePlanViewRepository
    {
        Task<List<UserWisePlanView>> GetAll();
        Task<UserWisePlanView?> GetByID(int id);
        Task<List<UserWisePlanView>> GetByUserID(int userid);
        Task<int> Insert(UserWisePlanView entity);
        Task<int> Update(UserWisePlanView entity);
        Task<int> Delete(int id);
    }
}

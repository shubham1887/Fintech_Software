using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserBankDetailsRepository
    {
        Task<List<UserBankDetails>> GetAll();
        Task<UserBankDetails?> GetByID(int id);
        Task<List<UserBankDetails>> GetByUserID(int userID);
        Task<int> Insert(UserBankDetails entity);
        Task<int> Update(UserBankDetails entity);
        Task<int> Delete(int id);
    }
}

using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserMerchantMasterRepository
    {
        Task<List<UserMerchantMaster>> GetAll();
        Task<UserMerchantMaster?> GetByID(int id);
        Task<List<UserMerchantMaster>> GetByUserID(int userid);
        Task<int> Insert(UserMerchantMaster entity);
        Task<int> Update(UserMerchantMaster entity);
        Task<int> Delete(int id);
    }
}

using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IOTPMasterRepository
    {
        Task<List<OTPMaster>> GetAll();
        Task<List<OTPMaster>> GetByUserID(int userID);
        Task<OTPMaster?> GetByID(int id);
        Task<int> Insert(OTPMaster entity);
        Task<int> Update(OTPMaster entity);
        Task<int> Delete(int id);
        Task<OTPMaster?> GetActiveByUserIdAndType(int userId, string type);
        Task<OTPMaster?> GetActiveByMobileAndType(string mobile, string type);
    }
}

using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IKYCApprovedDetailsRepository
    {
        Task<List<KYCApprovedDetails>> GetAll();
        Task<KYCApprovedDetails?> GetByID(int id);
        Task<List<KYCApprovedDetails>> GetByUserID(int userID);
        Task<int> Insert(KYCApprovedDetails entity);
        Task<int> Update(KYCApprovedDetails entity);
        Task<int> Delete(int id);
    }
}

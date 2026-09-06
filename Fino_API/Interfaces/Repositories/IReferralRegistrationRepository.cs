using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IReferralRegistrationRepository
    {
        Task<List<ReferralRegistration>> GetAll();
        Task<ReferralRegistration?> GetByID(int id);
        Task<List<ReferralRegistration>> GetByUserID(int userid);
        Task<int> Insert(ReferralRegistration entity);
        Task<int> Update(ReferralRegistration entity);
        Task<int> Delete(int id);
    }
}

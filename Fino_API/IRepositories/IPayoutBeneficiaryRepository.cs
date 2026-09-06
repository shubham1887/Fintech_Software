using FintechApp.Entities;

namespace FintechApp.IRepositories
{
    public interface IPayoutBeneficiaryRepository
    {
        Task<List<PayoutBeneficiary>> GetAll();
        Task<PayoutBeneficiary?> GetByID(int id);
        Task<List<PayoutBeneficiary>> GetByUserID(int userID);
        Task<int> Insert(PayoutBeneficiary entity);
        Task<int> Update(PayoutBeneficiary entity);
        Task<int> Delete(int id);
    }
}

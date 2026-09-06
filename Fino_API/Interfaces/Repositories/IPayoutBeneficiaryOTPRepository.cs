using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IPayoutBeneficiaryOTPRepository
    {
        Task<List<PayoutBeneficiaryOTP>> GetAll();
        Task<PayoutBeneficiaryOTP?> GetByID(int id);
        Task<List<PayoutBeneficiaryOTP>> GetByUserID(int userID);
        Task<int> Insert(PayoutBeneficiaryOTP entity);
        Task<int> Update(PayoutBeneficiaryOTP entity);
        Task<int> Delete(int id);
    }
}

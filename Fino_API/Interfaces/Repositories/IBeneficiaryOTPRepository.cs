using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IBeneficiaryOTPRepository
    {
        Task<List<BeneficiaryOTP>> GetAll();
        Task<BeneficiaryOTP?> GetByID(int id);
        Task<List<BeneficiaryOTP>> GetByUserID(int userid);
        Task<int> Insert(BeneficiaryOTP entity);
        Task<int> Update(BeneficiaryOTP entity);
        Task<int> Delete(int id);
    }
}

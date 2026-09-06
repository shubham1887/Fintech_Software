using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IPayoutVendorBeneficiaryMappingRepository
    {
        Task<List<PayoutVendorBeneficiaryMapping>> GetAll();
        Task<PayoutVendorBeneficiaryMapping?> GetByID(int id);
        Task<int> Insert(PayoutVendorBeneficiaryMapping entity);
        Task<int> Update(PayoutVendorBeneficiaryMapping entity);
        Task<int> Delete(int id);
    }
}

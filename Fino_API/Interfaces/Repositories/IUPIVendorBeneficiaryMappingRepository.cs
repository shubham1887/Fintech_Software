using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUPIVendorBeneficiaryMappingRepository
    {
        Task<List<UPIVendorBeneficiaryMapping>> GetAll();
        Task<UPIVendorBeneficiaryMapping?> GetByID(int id);

        Task<int> Insert(UPIVendorBeneficiaryMapping entity);
        Task<int> Update(UPIVendorBeneficiaryMapping entity);
        Task<int> Delete(int id);
    }
}

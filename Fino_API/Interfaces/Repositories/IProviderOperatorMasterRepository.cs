using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IProviderOperatorMasterRepository
    {
        Task<List<ProviderOperatorMaster>> GetAll();
        Task<List<ProviderOperatorMaster>> GetByProviderID(int providerId);
        Task<ProviderOperatorMaster?> GetByID(int id);
        Task<int> Insert(ProviderOperatorMaster entity);
        Task<int> Update(ProviderOperatorMaster entity);
        Task<int> Delete(int id);
    }
}

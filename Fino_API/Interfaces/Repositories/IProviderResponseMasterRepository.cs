using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IProviderResponseMasterRepository
    {
        Task<List<ProviderResponseMaster>> GetAll();
        Task<List<ProviderResponseMaster>> GetByProviderID(int providerId);
        Task<ProviderResponseMaster?> GetByID(int id);
        Task<int> Insert(ProviderResponseMaster entity);
        Task<int> Update(ProviderResponseMaster entity);
        Task<int> Delete(int id);
    }
}

using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IProviderResponseTypeMasterRepository
    {
        Task<List<ProviderResponseTypeMaster>> GetAll();
        Task<ProviderResponseTypeMaster?> GetByID(int id);
        Task<int> Insert(ProviderResponseTypeMaster entity);
        Task<int> Update(ProviderResponseTypeMaster entity);
        Task<int> Delete(int id);
    }
}

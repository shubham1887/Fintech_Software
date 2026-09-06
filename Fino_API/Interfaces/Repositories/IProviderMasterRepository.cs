using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IProviderMasterRepository
    {
        Task<List<ProviderMaster>> GetAll();
        Task<ProviderMaster?> GetByID(int id);
        Task<ProviderMaster?> GetByName(string name);
        Task<int> Insert(ProviderMaster entity);
        Task<int> Update(ProviderMaster entity);
        Task<int> Delete(int id);
    }
}

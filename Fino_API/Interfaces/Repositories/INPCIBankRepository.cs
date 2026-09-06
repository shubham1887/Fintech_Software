using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface INPCIBankRepository
    {
        Task<List<NPCIBank>> GetAll();
        Task<NPCIBank?> GetByID(int id);
        Task<int> Insert(NPCIBank entity);
        Task<int> Update(NPCIBank entity);
        Task<int> Delete(int id);
    }
}

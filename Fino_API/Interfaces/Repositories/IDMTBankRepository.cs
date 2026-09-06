using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IDMTBankRepository
    {
        Task<List<DMTBank>> GetAll();
        Task<DMTBank?> GetByID(int dmtBankId);
        Task<DMTBank?> GetByName(string name);
        Task<int> Insert(DMTBank entity);
        Task<int> Update(DMTBank entity);
        Task<int> Delete(int dmtBankId);
    }
}

using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IModeRepository
    {
        Task<List<Mode>> GetAll();
        Task<Mode?> GetByID(int modeId);
        Task<bool> ExistsByName(string modeName, int excludeModeId);
        Task<int> Insert(Mode entity);
        Task<int> Update(Mode entity);
        Task<int> Delete(int modeId);
    }
}

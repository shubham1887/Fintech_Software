using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface ISubModeRepository
    {
        Task<List<SubMode>> GetAll();
        Task<List<SubMode>> GetByModeID(int modeId);
        Task<SubMode?> GetByID(int subModeId);
        Task<bool> ExistsByName(int modeId, string subModeName, int excludeSubModeId);
        Task<bool> AnyByModeID(int modeId);
        Task<int> Insert(SubMode entity);
        Task<int> Update(SubMode entity);
        Task<int> Delete(int subModeId);
    }
}

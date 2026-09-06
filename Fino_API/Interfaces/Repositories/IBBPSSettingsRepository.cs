using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IBBPSSettingsRepository
    {
        Task<List<BBPSSettings>> GetAll();
        Task<BBPSSettings?> GetByID(int id);
        Task<int> Insert(BBPSSettings entity);
        Task<int> Update(BBPSSettings entity);
        Task<int> Delete(int id);
    }
}

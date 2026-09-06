using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IZoneMasterRepository
    {
        Task<List<ZoneMaster>> GetAll();
        Task<ZoneMaster?> GetByID(int id);
        Task<int> Insert(ZoneMaster entity);
        Task<int> Update(ZoneMaster entity);
        Task<int> Delete(int id);
    }
}

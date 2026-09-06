using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface ISmsMasterRepository
    {
        Task<List<SmsMaster>> GetAll();
        Task<SmsMaster?> GetByID(int smsMasterId);
        Task<int> Insert(SmsMaster entity);
        Task<int> Update(SmsMaster entity);
        Task<int> Delete(int smsMasterId);
    }
}

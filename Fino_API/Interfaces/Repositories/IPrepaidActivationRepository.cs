using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IPrepaidActivationRepository
    {
        Task<List<PrepaidActivation>> GetAll();
        Task<PrepaidActivation?> GetByID(int id);
        Task<List<PrepaidActivation>> GetByUserID(int userid);
        Task<int> Insert(PrepaidActivation entity);
        Task<int> Update(PrepaidActivation entity);
        Task<int> Delete(int id);
    }
}

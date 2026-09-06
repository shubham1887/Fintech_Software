using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IAuthorizationMasterRepository
    {
        Task<List<AuthorizationMaster>> GetAll();
        Task<AuthorizationMaster?> GetByID(int id);

        Task<int> Insert(AuthorizationMaster entity);
        Task<int> Update(AuthorizationMaster entity);
        Task<int> Delete(int id);
    }
}

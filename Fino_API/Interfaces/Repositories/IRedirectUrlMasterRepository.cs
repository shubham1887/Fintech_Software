using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IRedirectUrlMasterRepository
    {
        Task<List<RedirectUrlMaster>> GetAll();
        Task<RedirectUrlMaster?> GetByID(int id);
        Task<int> Insert(RedirectUrlMaster entity);
        Task<int> Update(RedirectUrlMaster entity);
        Task<int> Delete(int id);
    }
}

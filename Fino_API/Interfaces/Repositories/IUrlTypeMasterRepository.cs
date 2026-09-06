using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUrlTypeMasterRepository
    {
        Task<List<UrlTypeMaster>> GetAll();
        Task<UrlTypeMaster?> GetByID(int id);

        Task<int> Insert(UrlTypeMaster entity);
        Task<int> Update(UrlTypeMaster entity);
        Task<int> Delete(int id);
    }
}

using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IProviderUserMappingRepository
    {
        Task<List<ProviderUserMapping>> GetAll();
        Task<List<ProviderUserMapping>> GetByUserID(int userId);
        Task<ProviderUserMapping?> GetByID(int id);
        Task<int> Insert(ProviderUserMapping entity);
        Task<int> Update(ProviderUserMapping entity);
        Task<int> Delete(int id);
    }
}

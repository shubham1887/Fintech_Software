using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IRealProviderMappingRepository
    {
        Task<List<RealProviderMapping>> GetAll();
        Task<RealProviderMapping?> GetByID(int id);
        Task<int> Insert(RealProviderMapping entity);
        Task<int> Update(RealProviderMapping entity);
        Task<int> Delete(int id);
    }
}

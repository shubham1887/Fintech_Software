using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ICategoryChannelMappingRepository
    {
        Task<List<CategoryChannelMapping>> GetAll();
        Task<CategoryChannelMapping?> GetByID(int id);

        Task<int> Insert(CategoryChannelMapping entity);
        Task<int> Update(CategoryChannelMapping entity);
        Task<int> Delete(int id);
    }
}

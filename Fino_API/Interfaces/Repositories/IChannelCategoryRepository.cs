using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IChannelCategoryRepository
    {
        Task<List<ChannelCategory>> GetAll();
        Task<ChannelCategory?> GetByID(int id);

        Task<int> Insert(ChannelCategory entity);
        Task<int> Update(ChannelCategory entity);
        Task<int> Delete(int id);
    }
}

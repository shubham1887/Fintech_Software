using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IUserChannelSlabMappingRepository
    {
        Task<List<UserChannelSlabMapping>> GetAll();
        Task<List<UserChannelSlabMapping>> GetByUserID(int userId);
        Task<UserChannelSlabMapping?> GetByID(int userChannelSlabMappingId);
        Task<int> Insert(UserChannelSlabMapping entity);
        Task<int> Update(UserChannelSlabMapping entity);
        Task<int> Delete(int userChannelSlabMappingId);
    }
}

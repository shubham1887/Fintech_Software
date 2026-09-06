using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IChannelSlabMasterRepository
    {
        Task<List<ChannelSlabMaster>> GetAll();
        Task<List<ChannelSlabMaster>> GetByUplineUser(int uplineUserId);
        Task<ChannelSlabMaster?> GetByID(int channelSlabMasterId);
        Task<int> Insert(ChannelSlabMaster entity);
        Task<int> Update(ChannelSlabMaster entity);
        Task<int> Delete(int channelSlabMasterId);
    }
}

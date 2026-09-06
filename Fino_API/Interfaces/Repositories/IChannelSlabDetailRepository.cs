using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IChannelSlabDetailRepository
    {
        Task<List<ChannelSlabDetail>> GetAll();
        Task<List<ChannelSlabDetail>> GetByChannelSlabMasterID(int channelSlabMasterId);
        Task<ChannelSlabDetail?> GetByID(int channelSlabDetailId);
        Task<int> Insert(ChannelSlabDetail entity);
        Task<int> Update(ChannelSlabDetail entity);
        Task<int> Delete(int channelSlabDetailId);
    }
}

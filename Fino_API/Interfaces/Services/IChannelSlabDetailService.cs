using FintechApp.Requests.ChannelSlabDetail;
using FintechApp.Responses.ChannelSlabDetail;

namespace FintechApp.Interfaces.Services
{
    public interface IChannelSlabDetailService
    {
        Task<List<ChannelSlabDetailResponse>> GetAll();
        Task<List<ChannelSlabDetailResponse>> GetByChannelSlabMasterID(int channelSlabMasterId);
        Task<ChannelSlabDetailResponse?> GetByID(int channelSlabDetailId);
        Task<string> Create(CreateChannelSlabDetailRequest request);
        Task<string> Update(UpdateChannelSlabDetailRequest request);
        Task<string> Delete(int channelSlabDetailId);
    }
}

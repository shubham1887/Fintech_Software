using FintechApp.Requests.ChannelSlabMaster;
using FintechApp.Responses.ChannelSlabMaster;

namespace FintechApp.Interfaces.Services
{
    public interface IChannelSlabMasterService
    {
        Task<List<ChannelSlabMasterResponse>> GetAll();
        Task<List<ChannelSlabMasterResponse>> GetByUplineUser(int uplineUserId);
        Task<ChannelSlabMasterResponse?> GetByID(int channelSlabMasterId);
        Task<List<ChannelSlabMasterDropdownResponse>> GetDropdown(int uplineUserId);
        Task<string> Create(CreateChannelSlabMasterRequest request);
        Task<string> Update(UpdateChannelSlabMasterRequest request);
        Task<string> Delete(int channelSlabMasterId);
    }
}

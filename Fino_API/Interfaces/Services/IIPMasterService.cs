using FintechApp.Requests.IPMaster;
using FintechApp.Responses.IPMaster;

namespace FintechApp.Interfaces.Services
{
    public interface IIPMasterService
    {
        Task<IPMasterResponse?> GetIPMasterById(int ipMasterID);
        Task<List<IPMasterResponse>> GetIPMasters(GetIPMasterRequest request);

        Task<string> CreateIPMaster(CreateIPMasterRequest request);
        Task<string> UpdateIPMaster(UpdateIPMasterRequest request);
        Task<string> DeleteIPMaster(DeleteIPMasterRequest request);

    }
}

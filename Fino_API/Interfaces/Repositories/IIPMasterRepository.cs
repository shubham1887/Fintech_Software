using FintechApp.Entities;
using FintechApp.Requests.IPMaster;

namespace FintechApp.Interfaces.Repositories
{
    public interface IIPMasterRepository
    {
        Task<List<IPMaster>> GetAllIPMasters();
        Task<IPMaster?> GetIPMasterById(int ipMasterID);
        Task<IPMaster?> GetIPMasterByIP(string ip);
        Task<List<IPMaster>> GetIPMasters(GetIPMasterRequest request);
        Task<int> Insert(IPMaster ipMaster);
        Task<int> Update(IPMaster ipMaster);
        Task<int> Delete(IPMaster ipMaster);
    }
}
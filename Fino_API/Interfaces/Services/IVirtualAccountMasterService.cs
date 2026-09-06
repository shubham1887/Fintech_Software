using FintechApp.Requests.VirtualAccountMaster;
using FintechApp.Responses.VirtualAccountMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IVirtualAccountMasterService
    {
        Task<List<VirtualAccountMasterResponse>> GetAll();
        Task<VirtualAccountMasterResponse?> GetByID(int id);

        Task<string> Create(CreateVirtualAccountMasterRequest request);
        Task<string> Update(UpdateVirtualAccountMasterRequest request);
        Task<string> Delete(int id);
    }
}

using FintechApp.Requests.VoucherMaster;
using FintechApp.Responses.VoucherMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IVoucherMasterService
    {
        Task<List<VoucherMasterResponse>> GetAll();
        Task<List<VoucherMasterResponse>> GetByUserID(int userId);
        Task<VoucherMasterResponse?> GetByID(int id);
        Task<string> Create(CreateVoucherMasterRequest request);
        Task<string> Update(UpdateVoucherMasterRequest request);
        Task<string> Delete(int id);
    }
}

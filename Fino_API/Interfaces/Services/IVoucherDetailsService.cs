using FintechApp.Requests.VoucherDetails;
using FintechApp.Responses.VoucherDetails;
namespace FintechApp.Interfaces.Services
{
    public interface IVoucherDetailsService
    {
        Task<List<VoucherDetailsResponse>> GetAll();
        Task<List<VoucherDetailsResponse>> GetByUserID(int userId);
        Task<VoucherDetailsResponse?> GetByID(int id);
        Task<string> Create(CreateVoucherDetailsRequest request);
        Task<string> Update(UpdateVoucherDetailsRequest request);
        Task<string> Delete(int id);
    }
}

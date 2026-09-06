using FintechApp.Requests.VoucherType;
using FintechApp.Responses.VoucherType;
namespace FintechApp.Interfaces.Services
{
    public interface IVoucherTypeService
    {
        Task<List<VoucherTypeResponse>> GetAll();
        Task<VoucherTypeResponse?> GetByID(int id);
        Task<string> Create(CreateVoucherTypeRequest request);
        Task<string> Update(UpdateVoucherTypeRequest request);
        Task<string> Delete(int id);
    }
}

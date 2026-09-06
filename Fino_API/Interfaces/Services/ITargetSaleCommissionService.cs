using FintechApp.Requests.TargetSaleCommission;
using FintechApp.Responses.TargetSaleCommission;
namespace FintechApp.Interfaces.Services
{
    public interface ITargetSaleCommissionService
    {
        Task<List<TargetSaleCommissionResponse>> GetAll();
        Task<TargetSaleCommissionResponse?> GetByID(int id);
        Task<List<TargetSaleCommissionResponse>> GetByUserID(int userid);
        Task<string> Create(CreateTargetSaleCommissionRequest request);
        Task<string> Update(UpdateTargetSaleCommissionRequest request);
        Task<string> Delete(int id);
    }
}

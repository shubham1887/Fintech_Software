using FintechApp.Requests.Redeemcommission;
using FintechApp.Responses.Redeemcommission;
namespace FintechApp.Interfaces.Services
{
    public interface IRedeemcommissionService
    {
        Task<List<RedeemcommissionResponse>> GetAll();
        Task<RedeemcommissionResponse?> GetByID(int id);
        Task<List<RedeemcommissionResponse>> GetByUserID(int userID);
        Task<string> Create(CreateRedeemcommissionRequest request);
        Task<string> Update(UpdateRedeemcommissionRequest request);
        Task<string> Delete(int id);
    }
}

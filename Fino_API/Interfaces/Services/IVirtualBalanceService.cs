using FintechApp.Requests.VirtualBalance;
using FintechApp.Responses.VirtualBalance;
namespace FintechApp.Interfaces.Services
{
    public interface IVirtualBalanceService
    {
        Task<List<VirtualBalanceResponse>> GetAll();
        Task<VirtualBalanceResponse?> GetByID(int id);

        Task<string> Create(CreateVirtualBalanceRequest request, int userID);
        Task<string> Update(UpdateVirtualBalanceRequest request);
        Task<string> Delete(int id);
    }
}

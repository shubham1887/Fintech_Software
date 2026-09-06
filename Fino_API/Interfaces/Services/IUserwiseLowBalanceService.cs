using FintechApp.Requests.UserwiseLowBalance;
using FintechApp.Responses.UserwiseLowBalance;
namespace FintechApp.Interfaces.Services
{
    public interface IUserwiseLowBalanceService
    {
        Task<List<UserwiseLowBalanceResponse>> GetAll();
        Task<List<UserwiseLowBalanceResponse>> GetByUserID(int userId);
        Task<UserwiseLowBalanceResponse?> GetByID(int id);
        Task<string> Create(CreateUserwiseLowBalanceRequest request);
        Task<string> Update(UpdateUserwiseLowBalanceRequest request);
        Task<string> Delete(int id);
    }
}

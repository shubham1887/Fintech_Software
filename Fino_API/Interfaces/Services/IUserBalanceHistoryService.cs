using FintechApp.Requests.UserBalanceHistory;
using FintechApp.Responses.UserBalanceHistory;
namespace FintechApp.Interfaces.Services
{
    public interface IUserBalanceHistoryService
    {
        Task<List<UserBalanceHistoryResponse>> GetAll();
        Task<List<UserBalanceHistoryResponse>> GetByUserID(int userId);
        Task<UserBalanceHistoryResponse?> GetByID(int id);
        Task<string> Create(CreateUserBalanceHistoryRequest request);
        Task<string> Update(UpdateUserBalanceHistoryRequest request);
        Task<string> Delete(int id);
    }
}

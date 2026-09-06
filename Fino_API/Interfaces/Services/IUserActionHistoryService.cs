using FintechApp.Requests.UserActionHistory;
using FintechApp.Responses.UserActionHistory;
namespace FintechApp.Interfaces.Services
{
    public interface IUserActionHistoryService
    {
        Task<List<UserActionHistoryResponse>> GetAll();
        Task<UserActionHistoryResponse?> GetByID(int id);
        Task<List<UserActionHistoryResponse>> GetByUserID(int userid);
        Task<string> Create(CreateUserActionHistoryRequest request);
        Task<string> Update(UpdateUserActionHistoryRequest request);
        Task<string> Delete(int id);
    }
}

using FintechApp.Requests.PasswordChangeHistory;
using FintechApp.Responses.PasswordChangeHistory;
namespace FintechApp.Interfaces.Services
{
    public interface IPasswordChangeHistoryService
    {
        Task<List<PasswordChangeHistoryResponse>> GetAll();
        Task<List<PasswordChangeHistoryResponse>> GetByUserID(int userId);
        Task<PasswordChangeHistoryResponse?> GetByID(int id);
        Task<string> Create(CreatePasswordChangeHistoryRequest request);
        Task<string> Update(UpdatePasswordChangeHistoryRequest request);
        Task<string> Delete(int id);
    }
}

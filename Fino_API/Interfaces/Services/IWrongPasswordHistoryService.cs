using FintechApp.Requests.WrongPasswordHistory;
using FintechApp.Responses.WrongPasswordHistory;
namespace FintechApp.Interfaces.Services
{
    public interface IWrongPasswordHistoryService
    {
        Task<List<WrongPasswordHistoryResponse>> GetAll();
        Task<WrongPasswordHistoryResponse?> GetByID(int id);
        Task<List<WrongPasswordHistoryResponse>> GetByUserID(int userid);
        Task<string> Create(CreateWrongPasswordHistoryRequest request);
        Task<string> Update(UpdateWrongPasswordHistoryRequest request);
        Task<string> Delete(int id);
    }
}

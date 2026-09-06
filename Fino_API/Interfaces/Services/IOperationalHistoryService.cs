using FintechApp.Requests.OperationalHistory;
using FintechApp.Responses.OperationalHistory;
namespace FintechApp.Interfaces.Services
{
    public interface IOperationalHistoryService
    {
        Task<List<OperationalHistoryResponse>> GetAll();
        Task<OperationalHistoryResponse?> GetByID(int id);
        Task<List<OperationalHistoryResponse>> GetByUserID(int userid);
        Task<string> Create(CreateOperationalHistoryRequest request);
        Task<string> Update(UpdateOperationalHistoryRequest request);
        Task<string> Delete(int id);
    }
}

using FintechApp.Requests.MPinHistory;
using FintechApp.Responses.MPinHistory;
namespace FintechApp.Interfaces.Services
{
    public interface IMPinHistoryService
    {
        Task<List<MPinHistoryResponse>> GetAll();
        Task<MPinHistoryResponse?> GetByID(int id);
        Task<List<MPinHistoryResponse>> GetByUserID(int userid);
        Task<string> Create(CreateMPinHistoryRequest request);
        Task<string> Update(UpdateMPinHistoryRequest request);
        Task<string> Delete(int id);
    }
}

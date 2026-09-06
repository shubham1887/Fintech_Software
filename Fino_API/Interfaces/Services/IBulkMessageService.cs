using FintechApp.Requests.BulkMessage;
using FintechApp.Responses.BulkMessage;
namespace FintechApp.Interfaces.Services
{
    public interface IBulkMessageService
    {
        Task<List<BulkMessageResponse>> GetAll();
        Task<BulkMessageResponse?> GetByID(int id);
        Task<List<BulkMessageResponse>> GetByUserID(int userID);
        Task<string> Create(CreateBulkMessageRequest request);
        Task<string> Update(UpdateBulkMessageRequest request);
        Task<string> Delete(int id);
    }
}

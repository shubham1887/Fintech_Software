using FintechApp.Requests.Inbox;
using FintechApp.Responses.Inbox;
namespace FintechApp.Interfaces.Services
{
    public interface IInboxService
    {
        Task<List<InboxResponse>> GetAll();
        Task<InboxResponse?> GetByID(int id);
        Task<List<InboxResponse>> GetByUserID(int userID);
        Task<string> Create(CreateInboxRequest request);
        Task<string> Update(UpdateInboxRequest request);
        Task<string> Delete(int id);
    }
}

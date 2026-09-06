using FintechApp.Requests.Sender;
using FintechApp.Responses.Sender;
namespace FintechApp.Interfaces.Services
{
    public interface ISenderService
    {
        Task<List<SenderResponse>> GetAll();
        Task<SenderResponse?> GetByID(int id);
        Task<List<SenderResponse>> GetByUserID(int userid);
        Task<string> Create(CreateSenderRequest request);
        Task<string> Update(UpdateSenderRequest request);
        Task<string> Delete(int id);
    }
}

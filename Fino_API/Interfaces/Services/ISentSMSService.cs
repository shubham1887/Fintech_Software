using FintechApp.Requests.SentSMS;
using FintechApp.Responses.SentSMS;
namespace FintechApp.Interfaces.Services
{
    public interface ISentSMSService
    {
        Task<List<SentSMSResponse>> GetAll();
        Task<SentSMSResponse?> GetByID(int id);
        Task<List<SentSMSResponse>> GetByUserID(int userID);
        Task<string> Create(CreateSentSMSRequest request);
        Task<string> Update(UpdateSentSMSRequest request);
        Task<string> Delete(int id);
    }
}

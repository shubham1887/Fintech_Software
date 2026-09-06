using FintechApp.Requests.UpiSender;
using FintechApp.Responses.UpiSender;
namespace FintechApp.Interfaces.Services
{
    public interface IUpiSenderService
    {
        Task<List<UpiSenderResponse>> GetAll();
        Task<UpiSenderResponse?> GetByID(int id);
        Task<List<UpiSenderResponse>> GetByUserID(int userid);
        Task<string> Create(CreateUpiSenderRequest request);
        Task<string> Update(UpdateUpiSenderRequest request);
        Task<string> Delete(int id);
    }
}

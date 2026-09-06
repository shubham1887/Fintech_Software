using FintechApp.Requests.DMTSender;
using FintechApp.Responses.DMTSender;

namespace FintechApp.Interfaces.Services
{
    public interface IDMTSenderService
    {
        Task<List<DMTSenderResponse>> GetAll(GetDMTSenderRequest request);
        Task<string> Create(CreateDMTSenderRequest request);
        Task<string> Update(UpdateDMTSenderRequest request);
        Task<string> Delete(DeleteDMTSenderRequest request);
    }
}

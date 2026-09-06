using FintechApp.Requests.EmailSetting;
using FintechApp.Responses.EmailSetting;

namespace FintechApp.Interfaces.Services
{
    public interface IEmailSettingService
    {
        Task<List<EmailSettingResponse>> GetAll();
        Task<EmailSettingResponse?> GetByID(int emailSettingId);
        Task<string> Create(CreateEmailSettingRequest request);
        Task<string> Update(UpdateEmailSettingRequest request);
        Task<string> Delete(int emailSettingId);
    }
}

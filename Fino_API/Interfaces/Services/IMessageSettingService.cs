using FintechApp.Requests.MessageSetting;
using FintechApp.Responses.MessageSetting;

namespace FintechApp.Interfaces.Services
{
    public interface IMessageSettingService
    {
        Task<List<MessageSettingResponse>> GetAll();
        Task<MessageSettingResponse?> GetByID(int messageSettingId);
        Task<string> Create(CreateMessageSettingRequest request);
        Task<string> Update(UpdateMessageSettingRequest request);
        Task<string> Delete(int messageSettingId);
    }
}

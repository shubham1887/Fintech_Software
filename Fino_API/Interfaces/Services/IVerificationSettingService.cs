using FintechApp.Requests.VerificationSetting;
using FintechApp.Responses.VerificationSetting;
namespace FintechApp.Interfaces.Services
{
    public interface IVerificationSettingService
    {
        Task<List<VerificationSettingResponse>> GetAll();
        Task<VerificationSettingResponse?> GetByID(int id);

        Task<string> Create(CreateVerificationSettingRequest request);
        Task<string> Update(UpdateVerificationSettingRequest request);
        Task<string> Delete(int id);
    }
}

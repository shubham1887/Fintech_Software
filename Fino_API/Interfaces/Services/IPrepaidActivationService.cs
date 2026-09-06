using FintechApp.Requests.PrepaidActivation;
using FintechApp.Responses.PrepaidActivation;
namespace FintechApp.Interfaces.Services
{
    public interface IPrepaidActivationService
    {
        Task<List<PrepaidActivationResponse>> GetAll();
        Task<PrepaidActivationResponse?> GetByID(int id);
        Task<List<PrepaidActivationResponse>> GetByUserID(int userid);
        Task<string> Create(CreatePrepaidActivationRequest request);
        Task<string> Update(UpdatePrepaidActivationRequest request);
        Task<string> Delete(int id);
    }
}

using FintechApp.Requests.ThreewayReconCompletion;
using FintechApp.Responses.ThreewayReconCompletion;
namespace FintechApp.Interfaces.Services
{
    public interface IThreewayReconCompletionService
    {
        Task<List<ThreewayReconCompletionResponse>> GetAll();
        Task<ThreewayReconCompletionResponse?> GetByID(int id);

        Task<string> Create(CreateThreewayReconCompletionRequest request);
        Task<string> Update(UpdateThreewayReconCompletionRequest request);
        Task<string> Delete(int id);
    }
}

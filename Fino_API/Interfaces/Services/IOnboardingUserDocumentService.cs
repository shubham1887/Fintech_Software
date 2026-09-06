using FintechApp.Requests.OnboardingUserDocument;
using FintechApp.Responses.OnboardingUserDocument;
namespace FintechApp.Interfaces.Services
{
    public interface IOnboardingUserDocumentService
    {
        Task<List<OnboardingUserDocumentResponse>> GetAll();
        Task<OnboardingUserDocumentResponse?> GetByID(int id);

        Task<string> Create(CreateOnboardingUserDocumentRequest request);
        Task<string> Update(UpdateOnboardingUserDocumentRequest request);
        Task<string> Delete(int id);
    }
}

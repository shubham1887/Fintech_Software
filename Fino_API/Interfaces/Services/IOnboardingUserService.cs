using FintechApp.Requests.OnboardingUser;
using FintechApp.Responses.OnboardingUser;
namespace FintechApp.Interfaces.Services
{
    public interface IOnboardingUserService
    {
        Task<List<OnboardingUserResponse>> GetAll();
        Task<OnboardingUserResponse?> GetByID(int id);

        Task<string> Create(CreateOnboardingUserRequest request);
        Task<string> Update(UpdateOnboardingUserRequest request);
        Task<string> Delete(int id);
    }
}

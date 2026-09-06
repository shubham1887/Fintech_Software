using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IOnboardingUserDocumentRepository
    {
        Task<List<OnboardingUserDocument>> GetAll();
        Task<OnboardingUserDocument?> GetByID(int id);

        Task<int> Insert(OnboardingUserDocument entity);
        Task<int> Update(OnboardingUserDocument entity);
        Task<int> Delete(int id);
    }
}

using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IOnboardingUserRepository
    {
        Task<List<OnboardingUser>> GetAll();
        Task<OnboardingUser?> GetByID(int id);

        Task<int> Insert(OnboardingUser entity);
        Task<int> Update(OnboardingUser entity);
        Task<int> Delete(int id);
    }
}

using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IVerificationSettingRepository
    {
        Task<List<VerificationSetting>> GetAll();
        Task<VerificationSetting?> GetByID(int id);

        Task<int> Insert(VerificationSetting entity);
        Task<int> Update(VerificationSetting entity);
        Task<int> Delete(int id);
    }
}

using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ILeadGenerationSettingRepository
    {
        Task<List<LeadGenerationSetting>> GetAll();
        Task<LeadGenerationSetting?> GetByID(int id);
        Task<int> Insert(LeadGenerationSetting entity);
        Task<int> Update(LeadGenerationSetting entity);
        Task<int> Delete(int id);
    }
}

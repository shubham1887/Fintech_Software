using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IPlanSettingRepository
    {
        Task<List<PlanSetting>> GetAll();
        Task<PlanSetting?> GetByID(int id);
        Task<int> Insert(PlanSetting entity);
        Task<int> Update(PlanSetting entity);
        Task<int> Delete(int id);
    }
}

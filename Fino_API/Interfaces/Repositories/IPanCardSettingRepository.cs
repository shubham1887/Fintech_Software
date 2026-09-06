using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IPanCardSettingRepository
    {
        Task<List<PanCardSetting>> GetAll();
        Task<PanCardSetting?> GetByID(int id);

        Task<int> Insert(PanCardSetting entity);
        Task<int> Update(PanCardSetting entity);
        Task<int> Delete(int id);
    }
}

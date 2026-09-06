using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUPITransferSettingRepository
    {
        Task<List<UPITransferSetting>> GetAll();
        Task<UPITransferSetting?> GetByID(int id);

        Task<int> Insert(UPITransferSetting entity);
        Task<int> Update(UPITransferSetting entity);
        Task<int> Delete(int id);
    }
}

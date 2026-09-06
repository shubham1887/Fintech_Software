using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IPOSSettingMasterRepository
    {
        Task<List<POSSettingMaster>> GetAll();
        Task<POSSettingMaster?> GetByID(int id);

        Task<int> Insert(POSSettingMaster entity);
        Task<int> Update(POSSettingMaster entity);
        Task<int> Delete(int id);
    }
}

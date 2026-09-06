using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IFingerPrintDeviceTypeRepository
    {
        Task<List<FingerPrintDeviceType>> GetAll();
        Task<FingerPrintDeviceType?> GetByID(int id);
        Task<int> Insert(FingerPrintDeviceType entity);
        Task<int> Update(FingerPrintDeviceType entity);
        Task<int> Delete(int id);
    }
}

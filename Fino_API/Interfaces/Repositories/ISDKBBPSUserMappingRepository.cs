using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ISDKBBPSUserMappingRepository
    {
        Task<List<SDKBBPSUserMapping>> GetAll();
        Task<SDKBBPSUserMapping?> GetByID(int id);
        Task<List<SDKBBPSUserMapping>> GetByUserID(int userid);
        Task<int> Insert(SDKBBPSUserMapping entity);
        Task<int> Update(SDKBBPSUserMapping entity);
        Task<int> Delete(int id);
    }
}

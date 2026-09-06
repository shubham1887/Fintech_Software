using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserWisePayementGatewaySettingRepository
    {
        Task<List<UserWisePayementGatewaySetting>> GetAll();
        Task<UserWisePayementGatewaySetting?> GetByID(int id);
        Task<List<UserWisePayementGatewaySetting>> GetByUserID(int userid);
        Task<int> Insert(UserWisePayementGatewaySetting entity);
        Task<int> Update(UserWisePayementGatewaySetting entity);
        Task<int> Delete(int id);
    }
}

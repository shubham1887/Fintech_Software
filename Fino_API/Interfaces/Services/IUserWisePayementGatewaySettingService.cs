using FintechApp.Requests.UserWisePayementGatewaySetting;
using FintechApp.Responses.UserWisePayementGatewaySetting;
namespace FintechApp.Interfaces.Services
{
    public interface IUserWisePayementGatewaySettingService
    {
        Task<List<UserWisePayementGatewaySettingResponse>> GetAll();
        Task<UserWisePayementGatewaySettingResponse?> GetByID(int id);
        Task<List<UserWisePayementGatewaySettingResponse>> GetByUserID(int userid);
        Task<string> Create(CreateUserWisePayementGatewaySettingRequest request);
        Task<string> Update(UpdateUserWisePayementGatewaySettingRequest request);
        Task<string> Delete(int id);
    }
}

using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserWisePayementGatewaySetting;
using FintechApp.Responses.UserWisePayementGatewaySetting;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class UserWisePayementGatewaySettingService : IUserWisePayementGatewaySettingService
    {
        private readonly IUserWisePayementGatewaySettingRepository _repository;
        public UserWisePayementGatewaySettingService(IUserWisePayementGatewaySettingRepository repository) { _repository = repository; }
        private static UserWisePayementGatewaySettingResponse MapToResponse(UserWisePayementGatewaySetting e) => new() { UserWisePayementGatewaySettingID = e.UserWisePayementGatewaySettingID, UserID = e.UserID, PaymentGatewaySettingID = e.PaymentGatewaySettingID, MinimumAmount = e.MinimumAmount, MaximumAmount = e.MaximumAmount, };
        public async Task<List<UserWisePayementGatewaySettingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UserWisePayementGatewaySettingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<UserWisePayementGatewaySettingResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateUserWisePayementGatewaySettingRequest request) { var entity = new UserWisePayementGatewaySetting { UserID = request.UserID, PaymentGatewaySettingID = request.PaymentGatewaySettingID, MinimumAmount = request.MinimumAmount, MaximumAmount = request.MaximumAmount, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateUserWisePayementGatewaySettingRequest request) { var entity = await _repository.GetByID(request.UserWisePayementGatewaySettingID); if (entity == null) return Messages.UserWisePayementGatewaySettingMessages.NotFound; entity.UserID = request.UserID; entity.PaymentGatewaySettingID = request.PaymentGatewaySettingID; entity.MinimumAmount = request.MinimumAmount; entity.MaximumAmount = request.MaximumAmount; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.UserWisePayementGatewaySettingMessages.NotFound; }
    }
}

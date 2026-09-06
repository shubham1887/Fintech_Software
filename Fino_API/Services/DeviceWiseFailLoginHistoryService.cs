using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.DeviceWiseFailLoginHistory;
using FintechApp.Responses.DeviceWiseFailLoginHistory;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class DeviceWiseFailLoginHistoryService : IDeviceWiseFailLoginHistoryService
    {
        private readonly IDeviceWiseFailLoginHistoryRepository _repository;
        public DeviceWiseFailLoginHistoryService(IDeviceWiseFailLoginHistoryRepository repository) { _repository = repository; }
        private static DeviceWiseFailLoginHistoryResponse MapToResponse(DeviceWiseFailLoginHistory e) => new() { DeviceWiseFailLoginHistoryID = e.DeviceWiseFailLoginHistoryID, UserName = e.UserName, DeviceID = e.DeviceID, };
        public async Task<List<DeviceWiseFailLoginHistoryResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<DeviceWiseFailLoginHistoryResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateDeviceWiseFailLoginHistoryRequest request) { var entity = new DeviceWiseFailLoginHistory { UserName = request.UserName, DeviceID = request.DeviceID, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateDeviceWiseFailLoginHistoryRequest request) { var entity = await _repository.GetByID(request.DeviceWiseFailLoginHistoryID); if (entity == null) return Messages.DeviceWiseFailLoginHistoryMessages.NotFound; entity.UserName = request.UserName; entity.DeviceID = request.DeviceID; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.DeviceWiseFailLoginHistoryMessages.NotFound; }
    }
}

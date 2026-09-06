using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.MPinHistory;
using FintechApp.Responses.MPinHistory;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class MPinHistoryService : IMPinHistoryService
    {
        private readonly IMPinHistoryRepository _repository;
        public MPinHistoryService(IMPinHistoryRepository repository) { _repository = repository; }
        private static MPinHistoryResponse MapToResponse(MPinHistory e) => new() { MPinHistoryID = e.MPinHistoryID, UserID = e.UserID, Mpin = e.Mpin, DeviceID = e.DeviceID, Device = e.Device, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<MPinHistoryResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<MPinHistoryResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<MPinHistoryResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateMPinHistoryRequest request) { var entity = new MPinHistory { UserID = request.UserID, Mpin = request.Mpin, DeviceID = request.DeviceID, Device = request.Device, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateMPinHistoryRequest request) { var entity = await _repository.GetByID(request.MPinHistoryID); if (entity == null) return Messages.MPinHistoryMessages.NotFound; entity.UserID = request.UserID; entity.Mpin = request.Mpin; entity.DeviceID = request.DeviceID; entity.Device = request.Device; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.MPinHistoryMessages.NotFound; }
    }
}

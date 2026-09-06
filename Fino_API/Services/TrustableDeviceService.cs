using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.TrustableDevice;
using FintechApp.Responses.TrustableDevice;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class TrustableDeviceService : ITrustableDeviceService
    {
        private readonly ITrustableDeviceRepository _repository;
        public TrustableDeviceService(ITrustableDeviceRepository repository) { _repository = repository; }
        private static TrustableDeviceResponse MapToResponse(TrustableDevice e) => new() { ID = e.ID, IPAddress = e.IPAddress, UserID = e.UserID, IsVerified = e.IsVerified, Browser = e.Browser, Device = e.Device, };
        public async Task<List<TrustableDeviceResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<TrustableDeviceResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<TrustableDeviceResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateTrustableDeviceRequest request) { var entity = new TrustableDevice { IPAddress = request.IPAddress, UserID = request.UserID, IsVerified = request.IsVerified, Browser = request.Browser, Device = request.Device, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateTrustableDeviceRequest request) { var entity = await _repository.GetByID(request.ID); if (entity == null) return Messages.TrustableDeviceMessages.NotFound; entity.IPAddress = request.IPAddress; entity.UserID = request.UserID; entity.IsVerified = request.IsVerified; entity.Browser = request.Browser; entity.Device = request.Device; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.TrustableDeviceMessages.NotFound; }
    }
}

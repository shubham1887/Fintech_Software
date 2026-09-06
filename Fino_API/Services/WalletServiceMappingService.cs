using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.WalletServiceMapping;
using FintechApp.Responses.WalletServiceMapping;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class WalletServiceMappingService : IWalletServiceMappingService
    {
        private readonly IWalletServiceMappingRepository _repository;
        public WalletServiceMappingService(IWalletServiceMappingRepository repository) { _repository = repository; }
        private static WalletServiceMappingResponse MapToResponse(WalletServiceMapping e) => new() { WalletServiceMappingID = e.WalletServiceMappingID, WalletID = e.WalletID, ServiceID = e.ServiceID, WalletName = e.WalletMaster?.Name ?? string.Empty, ServiceName = e.ServiceMaster?.Name ?? string.Empty, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime, };
        public async Task<List<WalletServiceMappingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<WalletServiceMappingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateWalletServiceMappingRequest request) { var entity = new WalletServiceMapping { WalletID = request.WalletID, ServiceID = request.ServiceID, DateTime = DateTime.UtcNow, UpdatedateTime = DateTime.UtcNow, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateWalletServiceMappingRequest request) { var entity = await _repository.GetByID(request.WalletServiceMappingID); if (entity == null) return Messages.WalletServiceMappingMessages.NotFound; entity.WalletID = request.WalletID; entity.ServiceID = request.ServiceID; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.WalletServiceMappingMessages.NotFound; }
    }
}

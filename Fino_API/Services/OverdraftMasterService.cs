using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.OverdraftMaster;
using FintechApp.Responses.OverdraftMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class OverdraftMasterService : IOverdraftMasterService
    {
        private readonly IOverdraftMasterRepository _repository;
        public OverdraftMasterService(IOverdraftMasterRepository repository) { _repository = repository; }
        private static OverdraftMasterResponse MapToResponse(OverdraftMaster e) => new() { OverdraftMasterID = e.OverdraftMasterID, UserID = e.UserID, Overdraft = e.Overdraft, MaxOverdraft = e.MaxOverdraft, WalletID = e.WalletID, IsActive = e.IsActive, };
        public async Task<List<OverdraftMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<OverdraftMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<OverdraftMasterResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateOverdraftMasterRequest request) { var entity = new OverdraftMaster { UserID = request.UserID, Overdraft = request.Overdraft, MaxOverdraft = request.MaxOverdraft, WalletID = request.WalletID, IsActive = request.IsActive, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateOverdraftMasterRequest request) { var entity = await _repository.GetByID(request.OverdraftMasterID); if (entity == null) return Messages.OverdraftMasterMessages.NotFound; entity.UserID = request.UserID; entity.Overdraft = request.Overdraft; entity.MaxOverdraft = request.MaxOverdraft; entity.WalletID = request.WalletID; entity.IsActive = request.IsActive; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.OverdraftMasterMessages.NotFound; }
    }
}

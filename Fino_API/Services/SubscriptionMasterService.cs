using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.SubscriptionMaster;
using FintechApp.Responses.SubscriptionMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class SubscriptionMasterService : ISubscriptionMasterService
    {
        private readonly ISubscriptionMasterRepository _repository;
        public SubscriptionMasterService(ISubscriptionMasterRepository repository) { _repository = repository; }
        private static SubscriptionMasterResponse MapToResponse(SubscriptionMaster e) => new() { SubscriptionMasterID = e.SubscriptionMasterID, StartDate = e.StartDate, EndDate = e.EndDate, UpdatedDOC = e.UpdatedDOC, UpdatedDOCInt = e.UpdatedDOCInt, IsActive = e.IsActive, };
        public async Task<List<SubscriptionMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<SubscriptionMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateSubscriptionMasterRequest request) { var entity = new SubscriptionMaster { StartDate = request.StartDate, EndDate = request.EndDate, UpdatedDOC = request.UpdatedDOC, UpdatedDOCInt = request.UpdatedDOCInt, IsActive = request.IsActive, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateSubscriptionMasterRequest request) { var entity = await _repository.GetByID(request.SubscriptionMasterID); if (entity == null) return Messages.SubscriptionMasterMessages.NotFound; entity.StartDate = request.StartDate; entity.EndDate = request.EndDate; entity.UpdatedDOC = request.UpdatedDOC; entity.UpdatedDOCInt = request.UpdatedDOCInt; entity.IsActive = request.IsActive; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.SubscriptionMasterMessages.NotFound; }
    }
}

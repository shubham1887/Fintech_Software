using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.TransactionManualUpdate;
using FintechApp.Responses.TransactionManualUpdate;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class TransactionManualUpdateService : ITransactionManualUpdateService
    {
        private readonly ITransactionManualUpdateRepository _repository;
        public TransactionManualUpdateService(ITransactionManualUpdateRepository repository) { _repository = repository; }
        private static TransactionManualUpdateResponse MapToResponse(TransactionManualUpdate e) => new() { TransactionManualUpdateID = e.TransactionManualUpdateID, TransactionMasterID = e.TransactionMasterID, ManualUpdatedBy = e.ManualUpdatedBy, ProviderrefNo = e.ProviderrefNo, OldStatus = e.OldStatus, NewStatus = e.NewStatus, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<TransactionManualUpdateResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<TransactionManualUpdateResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateTransactionManualUpdateRequest request) { var entity = new TransactionManualUpdate { TransactionMasterID = request.TransactionMasterID, ManualUpdatedBy = request.ManualUpdatedBy, ProviderrefNo = request.ProviderrefNo, OldStatus = request.OldStatus, NewStatus = request.NewStatus, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateTransactionManualUpdateRequest request) { var entity = await _repository.GetByID(request.TransactionManualUpdateID); if (entity == null) return Messages.TransactionManualUpdateMessages.NotFound; entity.TransactionMasterID = request.TransactionMasterID; entity.ManualUpdatedBy = request.ManualUpdatedBy; entity.ProviderrefNo = request.ProviderrefNo; entity.OldStatus = request.OldStatus; entity.NewStatus = request.NewStatus; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.TransactionManualUpdateMessages.NotFound; }
    }
}

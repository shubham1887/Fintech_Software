using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ThreewayReconCompletion;
using FintechApp.Responses.ThreewayReconCompletion;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class ThreewayReconCompletionService : IThreewayReconCompletionService
    {
        private readonly IThreewayReconCompletionRepository _repository;
        public ThreewayReconCompletionService(IThreewayReconCompletionRepository repository) { _repository = repository; }
        private static ThreewayReconCompletionResponse MapToResponse(ThreewayReconCompletion e) => new() { ThreewayReconCompletionID = e.ThreewayReconCompletionID, TransactionMasterID = e.TransactionMasterID, ServiceID = e.ServiceID, OperatorID = e.OperatorID, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<ThreewayReconCompletionResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<ThreewayReconCompletionResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateThreewayReconCompletionRequest request) { var entity = new ThreewayReconCompletion { TransactionMasterID = request.TransactionMasterID, ServiceID = request.ServiceID, OperatorID = request.OperatorID, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateThreewayReconCompletionRequest request) { var entity = await _repository.GetByID(request.ThreewayReconCompletionID); if (entity == null) return Messages.ThreewayReconCompletionMessages.NotFound; entity.TransactionMasterID = request.TransactionMasterID; entity.ServiceID = request.ServiceID; entity.OperatorID = request.OperatorID; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.ThreewayReconCompletionMessages.NotFound; }
    }
}

using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.TransactionProviderHistory;
using FintechApp.Responses.TransactionProviderHistory;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class TransactionProviderHistoryService : ITransactionProviderHistoryService
    {
        private readonly ITransactionProviderHistoryRepository _repository;
        public TransactionProviderHistoryService(ITransactionProviderHistoryRepository repository) { _repository = repository; }
        private static TransactionProviderHistoryResponse MapToResponse(TransactionProviderHistory e) => new() { RechargeProviderHistoryID = e.RechargeProviderHistoryID, ProviderID = e.ProviderID, OperatorID = e.OperatorID, TransactionMasterID = e.TransactionMasterID, ProviderRequestID = e.ProviderRequestID, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<TransactionProviderHistoryResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<TransactionProviderHistoryResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateTransactionProviderHistoryRequest request) { var entity = new TransactionProviderHistory { ProviderID = request.ProviderID, OperatorID = request.OperatorID, TransactionMasterID = request.TransactionMasterID, ProviderRequestID = request.ProviderRequestID, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateTransactionProviderHistoryRequest request) { var entity = await _repository.GetByID(request.RechargeProviderHistoryID); if (entity == null) return Messages.TransactionProviderHistoryMessages.NotFound; entity.ProviderID = request.ProviderID; entity.OperatorID = request.OperatorID; entity.TransactionMasterID = request.TransactionMasterID; entity.ProviderRequestID = request.ProviderRequestID; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.TransactionProviderHistoryMessages.NotFound; }
    }
}

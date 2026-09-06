using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.TransactionStatusChange;
using FintechApp.Responses.TransactionStatusChange;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class TransactionStatusChangeService : ITransactionStatusChangeService
    {
        private readonly ITransactionStatusChangeRepository _repository;
        public TransactionStatusChangeService(ITransactionStatusChangeRepository repository) { _repository = repository; }
        private static TransactionStatusChangeResponse MapToResponse(TransactionStatusChange e) => new() { TransactionStatusChangeID = e.TransactionStatusChangeID, UserID = e.UserID, TransactionMasterID = e.TransactionMasterID, TransactionDOCInt = e.TransactionDOCInt, OldStatus = e.OldStatus, NewStatus = e.NewStatus, Amount = e.Amount, Remark = e.Remark, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<TransactionStatusChangeResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<TransactionStatusChangeResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<TransactionStatusChangeResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateTransactionStatusChangeRequest request) { var entity = new TransactionStatusChange { UserID = request.UserID, TransactionMasterID = request.TransactionMasterID, TransactionDOCInt = request.TransactionDOCInt, OldStatus = request.OldStatus, NewStatus = request.NewStatus, Amount = request.Amount, Remark = request.Remark, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateTransactionStatusChangeRequest request) { var entity = await _repository.GetByID(request.TransactionStatusChangeID); if (entity == null) return Messages.TransactionStatusChangeMessages.NotFound; entity.UserID = request.UserID; entity.TransactionMasterID = request.TransactionMasterID; entity.TransactionDOCInt = request.TransactionDOCInt; entity.OldStatus = request.OldStatus; entity.NewStatus = request.NewStatus; entity.Amount = request.Amount; entity.Remark = request.Remark; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.TransactionStatusChangeMessages.NotFound; }
    }
}

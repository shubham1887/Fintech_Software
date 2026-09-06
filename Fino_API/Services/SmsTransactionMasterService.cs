using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.SmsTransactionMaster;
using FintechApp.Responses.SmsTransactionMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class SmsTransactionMasterService : ISmsTransactionMasterService
    {
        private readonly ISmsTransactionMasterRepository _repository;
        public SmsTransactionMasterService(ISmsTransactionMasterRepository repository) { _repository = repository; }
        private static SmsTransactionMasterResponse MapToResponse(SmsTransactionMaster e) => new() { SmsTransactionMasterID = e.SmsTransactionMasterID, TransactionMasterID = e.TransactionMasterID, ServiceID = e.ServiceID, Amount = e.Amount, BillNumber = e.BillNumber, IsProcess = e.IsProcess, IsSuccess = e.IsSuccess, };
        public async Task<List<SmsTransactionMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<SmsTransactionMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateSmsTransactionMasterRequest request) { var entity = new SmsTransactionMaster { TransactionMasterID = request.TransactionMasterID, ServiceID = request.ServiceID, Amount = request.Amount, BillNumber = request.BillNumber, IsProcess = request.IsProcess, IsSuccess = request.IsSuccess, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateSmsTransactionMasterRequest request) { var entity = await _repository.GetByID(request.SmsTransactionMasterID); if (entity == null) return Messages.SmsTransactionMasterMessages.NotFound; entity.TransactionMasterID = request.TransactionMasterID; entity.ServiceID = request.ServiceID; entity.Amount = request.Amount; entity.BillNumber = request.BillNumber; entity.IsProcess = request.IsProcess; entity.IsSuccess = request.IsSuccess; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.SmsTransactionMasterMessages.NotFound; }
    }
}

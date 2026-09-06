using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.BankRequestResponseLog;
using FintechApp.Responses.BankRequestResponseLog;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class BankRequestResponseLogService : IBankRequestResponseLogService
    {
        private readonly IBankRequestResponseLogRepository _repository;
        public BankRequestResponseLogService(IBankRequestResponseLogRepository repository) { _repository = repository; }
        private static BankRequestResponseLogResponse MapToResponse(BankRequestResponseLog e) => new() { BankRequestResponseLogID = e.BankRequestResponseLogID, UserID = e.UserID, ProviderID = e.ProviderID, TransactionID = e.TransactionID, ConsumerNumber = e.ConsumerNumber, Request = e.Request, Response = e.Response, IPAddress = e.IPAddress, Through = e.Through, ServiceID = e.ServiceID, };
        public async Task<List<BankRequestResponseLogResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<BankRequestResponseLogResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<BankRequestResponseLogResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateBankRequestResponseLogRequest request) { var entity = new BankRequestResponseLog { UserID = request.UserID, ProviderID = request.ProviderID, TransactionID = request.TransactionID, ConsumerNumber = request.ConsumerNumber, Request = request.Request, Response = request.Response, IPAddress = request.IPAddress, Through = request.Through, ServiceID = request.ServiceID, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateBankRequestResponseLogRequest request) { var entity = await _repository.GetByID(request.BankRequestResponseLogID); if (entity == null) return Messages.BankRequestResponseLogMessages.NotFound; entity.UserID = request.UserID; entity.ProviderID = request.ProviderID; entity.TransactionID = request.TransactionID; entity.ConsumerNumber = request.ConsumerNumber; entity.Request = request.Request; entity.Response = request.Response; entity.IPAddress = request.IPAddress; entity.Through = request.Through; entity.ServiceID = request.ServiceID; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.BankRequestResponseLogMessages.NotFound; }
    }
}

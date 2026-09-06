using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.TransactionAPIUserRequestMaster;
using FintechApp.Responses.TransactionAPIUserRequestMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class TransactionAPIUserRequestMasterService : ITransactionAPIUserRequestMasterService
    {
        private readonly ITransactionAPIUserRequestMasterRepository _repository;
        public TransactionAPIUserRequestMasterService(ITransactionAPIUserRequestMasterRepository repository) { _repository = repository; }
        private static TransactionAPIUserRequestMasterResponse MapToResponse(TransactionAPIUserRequestMaster e) => new() { TransactionAPIUserRequestMasterID = e.TransactionAPIUserRequestMasterID, UserID = e.UserID, ApiUserRequestID = e.ApiUserRequestID, };
        public async Task<List<TransactionAPIUserRequestMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<TransactionAPIUserRequestMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<TransactionAPIUserRequestMasterResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateTransactionAPIUserRequestMasterRequest request) { var entity = new TransactionAPIUserRequestMaster { UserID = request.UserID, ApiUserRequestID = request.ApiUserRequestID, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateTransactionAPIUserRequestMasterRequest request) { var entity = await _repository.GetByID(request.TransactionAPIUserRequestMasterID); if (entity == null) return Messages.TransactionAPIUserRequestMasterMessages.NotFound; entity.UserID = request.UserID; entity.ApiUserRequestID = request.ApiUserRequestID; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.TransactionAPIUserRequestMasterMessages.NotFound; }
    }
}

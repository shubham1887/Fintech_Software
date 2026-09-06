using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.eKYCLog;
using FintechApp.Responses.eKYCLog;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class eKYCLogService : IeKYCLogService
    {
        private readonly IeKYCLogRepository _repository;
        public eKYCLogService(IeKYCLogRepository repository) { _repository = repository; }
        private static eKYCLogResponse MapToResponse(eKYCLog e) => new() { eKYCLogID = e.eKYCLogID, DocumentNo = e.DocumentNo, UserID = e.UserID, Response = e.Response, DocumentType = e.DocumentType, };
        public async Task<List<eKYCLogResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<eKYCLogResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<eKYCLogResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateeKYCLogRequest request) { var entity = new eKYCLog { DocumentNo = request.DocumentNo, UserID = request.UserID, Response = request.Response, DocumentType = request.DocumentType, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateeKYCLogRequest request) { var entity = await _repository.GetByID(request.eKYCLogID); if (entity == null) return Messages.eKYCLogMessages.NotFound; entity.DocumentNo = request.DocumentNo; entity.UserID = request.UserID; entity.Response = request.Response; entity.DocumentType = request.DocumentType; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.eKYCLogMessages.NotFound; }
    }
}

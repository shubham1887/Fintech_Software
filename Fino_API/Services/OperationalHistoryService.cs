using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.OperationalHistory;
using FintechApp.Responses.OperationalHistory;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class OperationalHistoryService : IOperationalHistoryService
    {
        private readonly IOperationalHistoryRepository _repository;
        public OperationalHistoryService(IOperationalHistoryRepository repository) { _repository = repository; }
        private static OperationalHistoryResponse MapToResponse(OperationalHistory e) => new() { OperationalHistoryID = e.OperationalHistoryID, UserID = e.UserID, APIType = e.APIType, DocNo = e.DocNo, };
        public async Task<List<OperationalHistoryResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<OperationalHistoryResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<OperationalHistoryResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateOperationalHistoryRequest request) { var entity = new OperationalHistory { UserID = request.UserID, APIType = request.APIType, DocNo = request.DocNo, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateOperationalHistoryRequest request) { var entity = await _repository.GetByID(request.OperationalHistoryID); if (entity == null) return Messages.OperationalHistoryMessages.NotFound; entity.UserID = request.UserID; entity.APIType = request.APIType; entity.DocNo = request.DocNo; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.OperationalHistoryMessages.NotFound; }
    }
}

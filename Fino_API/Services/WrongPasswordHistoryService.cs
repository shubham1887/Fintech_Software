using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.WrongPasswordHistory;
using FintechApp.Responses.WrongPasswordHistory;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class WrongPasswordHistoryService : IWrongPasswordHistoryService
    {
        private readonly IWrongPasswordHistoryRepository _repository;
        public WrongPasswordHistoryService(IWrongPasswordHistoryRepository repository) { _repository = repository; }
        private static WrongPasswordHistoryResponse MapToResponse(WrongPasswordHistory e) => new() { WrongPasswordHistoryID = e.WrongPasswordHistoryID, UserID = e.UserID, MobileNumber = e.MobileNumber, IPAddress = e.IPAddress, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<WrongPasswordHistoryResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<WrongPasswordHistoryResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<WrongPasswordHistoryResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateWrongPasswordHistoryRequest request) { var entity = new WrongPasswordHistory { UserID = request.UserID, MobileNumber = request.MobileNumber, IPAddress = request.IPAddress, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateWrongPasswordHistoryRequest request) { var entity = await _repository.GetByID(request.WrongPasswordHistoryID); if (entity == null) return Messages.WrongPasswordHistoryMessages.NotFound; entity.UserID = request.UserID; entity.MobileNumber = request.MobileNumber; entity.IPAddress = request.IPAddress; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.WrongPasswordHistoryMessages.NotFound; }
    }
}

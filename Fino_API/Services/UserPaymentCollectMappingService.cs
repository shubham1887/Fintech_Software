using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserPaymentCollectMapping;
using FintechApp.Responses.UserPaymentCollectMapping;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class UserPaymentCollectMappingService : IUserPaymentCollectMappingService
    {
        private readonly IUserPaymentCollectMappingRepository _repository;
        public UserPaymentCollectMappingService(IUserPaymentCollectMappingRepository repository) { _repository = repository; }
        private static UserPaymentCollectMappingResponse MapToResponse(UserPaymentCollectMapping e) => new() { UserPaymentCollectMappingID = e.UserPaymentCollectMappingID, ProviderName = e.ProviderName, UserID = e.UserID, Key = e.Key, Value = e.Value, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<UserPaymentCollectMappingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UserPaymentCollectMappingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<UserPaymentCollectMappingResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateUserPaymentCollectMappingRequest request) { var entity = new UserPaymentCollectMapping { ProviderName = request.ProviderName, UserID = request.UserID, Key = request.Key, Value = request.Value, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateUserPaymentCollectMappingRequest request) { var entity = await _repository.GetByID(request.UserPaymentCollectMappingID); if (entity == null) return Messages.UserPaymentCollectMappingMessages.NotFound; entity.ProviderName = request.ProviderName; entity.UserID = request.UserID; entity.Key = request.Key; entity.Value = request.Value; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.UserPaymentCollectMappingMessages.NotFound; }
    }
}

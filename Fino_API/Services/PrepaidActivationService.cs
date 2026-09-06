using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.PrepaidActivation;
using FintechApp.Responses.PrepaidActivation;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class PrepaidActivationService : IPrepaidActivationService
    {
        private readonly IPrepaidActivationRepository _repository;
        public PrepaidActivationService(IPrepaidActivationRepository repository) { _repository = repository; }
        private static PrepaidActivationResponse MapToResponse(PrepaidActivation e) => new() { PrepaidActivationID = e.PrepaidActivationID, OperatorID = e.OperatorID, UserID = e.UserID, ActivationType = e.ActivationType, AlternateNumber = e.AlternateNumber, Name = e.Name, Address = e.Address, Pincode = e.Pincode, Status = e.Status, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<PrepaidActivationResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<PrepaidActivationResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<PrepaidActivationResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreatePrepaidActivationRequest request) { var entity = new PrepaidActivation { OperatorID = request.OperatorID, UserID = request.UserID, ActivationType = request.ActivationType, AlternateNumber = request.AlternateNumber, Name = request.Name, Address = request.Address, Pincode = request.Pincode, Status = request.Status, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdatePrepaidActivationRequest request) { var entity = await _repository.GetByID(request.PrepaidActivationID); if (entity == null) return Messages.PrepaidActivationMessages.NotFound; entity.OperatorID = request.OperatorID; entity.UserID = request.UserID; entity.ActivationType = request.ActivationType; entity.AlternateNumber = request.AlternateNumber; entity.Name = request.Name; entity.Address = request.Address; entity.Pincode = request.Pincode; entity.Status = request.Status; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.PrepaidActivationMessages.NotFound; }
    }
}

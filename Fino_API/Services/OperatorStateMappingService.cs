using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.OperatorStateMapping;
using FintechApp.Responses.OperatorStateMapping;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class OperatorStateMappingService : IOperatorStateMappingService
    {
        private readonly IOperatorStateMappingRepository _repository;
        public OperatorStateMappingService(IOperatorStateMappingRepository repository) { _repository = repository; }

        private static OperatorStateMappingResponse MapToResponse(OperatorStateMapping e) => new()
        {
            OperatorStateMappingID = e.OperatorStateMappingID,
            OperatorID = e.OperatorID,
            OperatorName = e.OperatorMaster?.OperatorName,
            ZoneMasterID = e.ZoneMasterID,
            HLR = e.HLR,
            IsActive = e.IsActive,
            DateTime = e.DateTime
        };

        public async Task<List<OperatorStateMappingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<OperatorStateMappingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<OperatorStateMappingResponse>> GetByOperatorID(int operatorID) { var list = await _repository.GetByOperatorID(operatorID); return list.Select(MapToResponse).ToList(); }

        public async Task<string> Create(CreateOperatorStateMappingRequest request)
        {
            var entity = new OperatorStateMapping
            {
                OperatorID = request.OperatorID,
                ZoneMasterID = request.ZoneMasterID,
                HLR = request.HLR,
                IsActive = request.IsActive,
                DateTime = DateTime.UtcNow
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateOperatorStateMappingRequest request)
        {
            var entity = await _repository.GetByID(request.OperatorStateMappingID);
            if (entity == null) return Messages.OperatorStateMappingMessages.NotFound;
            entity.OperatorID = request.OperatorID;
            entity.ZoneMasterID = request.ZoneMasterID;
            entity.HLR = request.HLR;
            entity.IsActive = request.IsActive;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.OperatorStateMappingMessages.NotFound;
        }
    }
}

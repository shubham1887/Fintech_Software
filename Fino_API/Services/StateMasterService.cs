using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.State;
using FintechApp.Responses.State;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class StateMasterService : IStateMasterService
    {
        private readonly IStateMasterRepository _repository;

        public StateMasterService(IStateMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<StateResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<StateResponse?> GetByID(int stateId)
        {
            var entity = await _repository.GetByID(stateId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<List<StateDropdownResponse>> GetDropdown()
        {
            var all = await _repository.GetAll();
            return all.Where(x => x.IsActive)
                      .Select(x => new StateDropdownResponse
                      {
                          StateID = x.StateID,
                          StateName = x.StateName,
                          StateCode = x.StateCode
                      }).ToList();
        }

        public async Task<string> Create(CreateStateRequest request)
        {
            var existing = await _repository.GetByName(request.StateName);
            if (existing != null)
                return Messages.StateMasterMessages.AlreadyExists;

            var entity = new StateMaster
            {
                StateName = request.StateName,
                StateCode = request.StateCode,
                IsActive = request.IsActive,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateStateRequest request)
        {
            var entity = await _repository.GetByID(request.StateID);
            if (entity == null) return Messages.StateMasterMessages.NotFound;

            entity.StateName = request.StateName;
            entity.StateCode = request.StateCode;
            entity.IsActive = request.IsActive;
            entity.UpdatedateTime = DateTime.UtcNow;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int stateId)
        {
            var entity = await _repository.GetByID(stateId);
            if (entity == null) return Messages.StateMasterMessages.NotFound;

            int result = await _repository.Delete(stateId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static StateResponse MapToResponse(StateMaster e) => new()
        {
            StateID = e.StateID,
            StateName = e.StateName,
            StateCode = e.StateCode,
            IsActive = e.IsActive,
            DateTime = e.DateTime
        };
    }
}

using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.CallbackMaster;
using FintechApp.Responses.CallbackMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class CallbackMasterService : ICallbackMasterService
    {
        private readonly ICallbackMasterRepository _repository;

        public CallbackMasterService(ICallbackMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<CallbackMasterResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<CallbackMasterResponse>> GetByUserID(int userId)
        {
            var list = await _repository.GetByUserID(userId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<CallbackMasterResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateCallbackMasterRequest request)
        {
            var entity = new CallbackMaster
            {
                UserID = request.UserID,
                CallbackURL = request.CallbackURL,
                IsDelete = request.IsDelete,
                ServiceID = request.ServiceID,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateCallbackMasterRequest request)
        {
            var entity = await _repository.GetByID(request.CallbackMasterID);
            if (entity == null) return CallbackMasterMessages.NotFound;
            entity.UserID = request.UserID;
            entity.CallbackURL = request.CallbackURL;
            entity.IsDelete = request.IsDelete;
            entity.ServiceID = request.ServiceID;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : CallbackMasterMessages.NotFound;
        }

        private static CallbackMasterResponse MapToResponse(CallbackMaster e) => new()
        {
            CallbackMasterID = e.CallbackMasterID,
            UserID = e.UserID,
            CallbackURL = e.CallbackURL,
            IsDelete = e.IsDelete,
            ServiceID = e.ServiceID,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };
    }
}

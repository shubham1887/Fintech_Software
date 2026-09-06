using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.APIUserCallbackRequestResponseLog;
using FintechApp.Responses.APIUserCallbackRequestResponseLog;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class APIUserCallbackRequestResponseLogService : IAPIUserCallbackRequestResponseLogService
    {
        private readonly IAPIUserCallbackRequestResponseLogRepository _repository;
        public APIUserCallbackRequestResponseLogService(IAPIUserCallbackRequestResponseLogRepository repository) { _repository = repository; }

        private static APIUserCallbackRequestResponseLogResponse MapToResponse(APIUserCallbackRequestResponseLog e) => new()
        {
            APIUserCallbackRequestResponseLogID = e.APIUserCallbackRequestResponseLogID,
            UserID = e.UserID,
            Request = e.Request,
            Response = e.Response,
            TransactionMasterID = e.TransactionMasterID,
            APIUserRequestID = e.APIUserRequestID,
            ConsumerNumber = e.ConsumerNumber,
        };

        public async Task<List<APIUserCallbackRequestResponseLogResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<APIUserCallbackRequestResponseLogResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<APIUserCallbackRequestResponseLogResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateAPIUserCallbackRequestResponseLogRequest request)
        {
            var entity = new APIUserCallbackRequestResponseLog
            {
                UserID = request.UserID,
                Request = request.Request,
                Response = request.Response,
                TransactionMasterID = request.TransactionMasterID,
                APIUserRequestID = request.APIUserRequestID,
                ConsumerNumber = request.ConsumerNumber,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateAPIUserCallbackRequestResponseLogRequest request)
        {
            var entity = await _repository.GetByID(request.APIUserCallbackRequestResponseLogID);
            if (entity == null) return Messages.APIUserCallbackRequestResponseLogMessages.NotFound;
            entity.UserID = request.UserID;
            entity.Request = request.Request;
            entity.Response = request.Response;
            entity.TransactionMasterID = request.TransactionMasterID;
            entity.APIUserRequestID = request.APIUserRequestID;
            entity.ConsumerNumber = request.ConsumerNumber;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.APIUserCallbackRequestResponseLogMessages.NotFound;
        }
    }
}

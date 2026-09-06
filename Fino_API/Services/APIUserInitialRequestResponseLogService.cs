using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.APIUserInitialRequestResponseLog;
using FintechApp.Responses.APIUserInitialRequestResponseLog;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class APIUserInitialRequestResponseLogService : IAPIUserInitialRequestResponseLogService
    {
        private readonly IAPIUserInitialRequestResponseLogRepository _repository;
        public APIUserInitialRequestResponseLogService(IAPIUserInitialRequestResponseLogRepository repository) { _repository = repository; }

        private static APIUserInitialRequestResponseLogResponse MapToResponse(APIUserInitialRequestResponseLog e) => new()
        {
            APIUserInitialRequestResponseLogID = e.APIUserInitialRequestResponseLogID,
            UserID = e.UserID,
            Request = e.Request,
            Response = e.Response,
            TransactionMasterID = e.TransactionMasterID,
            APIUserRequestID = e.APIUserRequestID,
            ConsumerNumber = e.ConsumerNumber,
        };

        public async Task<List<APIUserInitialRequestResponseLogResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<APIUserInitialRequestResponseLogResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<APIUserInitialRequestResponseLogResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateAPIUserInitialRequestResponseLogRequest request)
        {
            var entity = new APIUserInitialRequestResponseLog
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

        public async Task<string> Update(UpdateAPIUserInitialRequestResponseLogRequest request)
        {
            var entity = await _repository.GetByID(request.APIUserInitialRequestResponseLogID);
            if (entity == null) return Messages.APIUserInitialRequestResponseLogMessages.NotFound;
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
            return result > 0 ? StatusType.Ok : Messages.APIUserInitialRequestResponseLogMessages.NotFound;
        }
    }
}

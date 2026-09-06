using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ProviderCallBackLog;
using FintechApp.Responses.ProviderCallBackLog;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ProviderCallBackLogService : IProviderCallBackLogService
    {
        private readonly IProviderCallBackLogRepository _repository;
        public ProviderCallBackLogService(IProviderCallBackLogRepository repository) { _repository = repository; }

        private static ProviderCallBackLogResponse MapToResponse(ProviderCallBackLog e) => new()
        {
            ProviderCallBackID = e.ProviderCallBackID,
            ProviderReply = e.ProviderReply,
            ProviderID = e.ProviderID,
            Type = e.Type,
            TransactionMasterID = e.TransactionMasterID,
            DateTime = e.DateTime,
        };

        public async Task<List<ProviderCallBackLogResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<List<ProviderCallBackLogResponse>> GetByTransaction(int transactionMasterId) { var list = await _repository.GetByTransaction(transactionMasterId); return list.Select(MapToResponse).ToList(); }
        public async Task<ProviderCallBackLogResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateProviderCallBackLogRequest request)
        {
            var entity = new ProviderCallBackLog
            {
                ProviderReply = request.ProviderReply,
                ProviderID = request.ProviderID,
                Type = request.Type,
                TransactionMasterID = request.TransactionMasterID,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateProviderCallBackLogRequest request)
        {
            var entity = await _repository.GetByID(request.ProviderCallBackID);
            if (entity == null) return Messages.ProviderCallBackLogMessages.NotFound;
            entity.ProviderReply = request.ProviderReply;
            entity.ProviderID = request.ProviderID;
            entity.Type = request.Type;
            entity.TransactionMasterID = request.TransactionMasterID;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.ProviderCallBackLogMessages.NotFound;
        }
    }
}

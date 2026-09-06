using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.PasswordChangeHistory;
using FintechApp.Responses.PasswordChangeHistory;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class PasswordChangeHistoryService : IPasswordChangeHistoryService
    {
        private readonly IPasswordChangeHistoryRepository _repository;

        public PasswordChangeHistoryService(IPasswordChangeHistoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<PasswordChangeHistoryResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<PasswordChangeHistoryResponse>> GetByUserID(int userId)
        {
            var list = await _repository.GetByUserID(userId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<PasswordChangeHistoryResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreatePasswordChangeHistoryRequest request)
        {
            var entity = new PasswordChangeHistory
            {
                UserID = request.UserID,
                Type = request.Type,
                IPAddress = request.IPAddress,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdatePasswordChangeHistoryRequest request)
        {
            var entity = await _repository.GetByID(request.PasswordChangeHistoryID);
            if (entity == null) return PasswordChangeHistoryMessages.NotFound;
            entity.UserID = request.UserID;
            entity.Type = request.Type;
            entity.IPAddress = request.IPAddress;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : PasswordChangeHistoryMessages.NotFound;
        }

        private static PasswordChangeHistoryResponse MapToResponse(PasswordChangeHistory e) => new()
        {
            PasswordChangeHistoryID = e.PasswordChangeHistoryID,
            UserID = e.UserID,
            Type = e.Type,
            IPAddress = e.IPAddress,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };
    }
}

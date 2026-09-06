using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.OperatorUserwise;
using FintechApp.Responses.OperatorUserwise;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class OperatorUserwiseService : IOperatorUserwiseService
    {
        private readonly IOperatorUserwiseRepository _repository;

        public OperatorUserwiseService(IOperatorUserwiseRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<OperatorUserwiseResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<OperatorUserwiseResponse>> GetByUserID(int userId)
        {
            var list = await _repository.GetByUserID(userId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<OperatorUserwiseResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateOperatorUserwiseRequest request)
        {
            var entity = new OperatorUserwise
            {
                UserID = request.UserID,
                OperatorID = request.OperatorID,
                IsActive = request.IsActive,
                UsageLimit = request.UsageLimit,
                ServiceID = request.ServiceID,
                BlockedAmount = request.BlockedAmount,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateOperatorUserwiseRequest request)
        {
            var entity = await _repository.GetByID(request.OperatorUserwiseID);
            if (entity == null) return OperatorUserwiseMessages.NotFound;
            entity.UserID = request.UserID;
            entity.OperatorID = request.OperatorID;
            entity.IsActive = request.IsActive;
            entity.UsageLimit = request.UsageLimit;
            entity.ServiceID = request.ServiceID;
            entity.BlockedAmount = request.BlockedAmount;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : OperatorUserwiseMessages.NotFound;
        }

        private static OperatorUserwiseResponse MapToResponse(OperatorUserwise e) => new()
        {
            OperatorUserwiseID = e.OperatorUserwiseID,
            UserID = e.UserID,
            OperatorID = e.OperatorID,
            IsActive = e.IsActive,
            UsageLimit = e.UsageLimit,
            ServiceID = e.ServiceID,
            BlockedAmount = e.BlockedAmount,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };
    }
}

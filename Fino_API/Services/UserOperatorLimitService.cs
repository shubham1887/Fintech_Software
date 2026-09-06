using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.UserOperatorLimit;
using FintechApp.Responses.UserOperatorLimit;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class UserOperatorLimitService : IUserOperatorLimitService
    {
        private readonly IUserOperatorLimitRepository _repository;

        public UserOperatorLimitService(IUserOperatorLimitRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserOperatorLimitResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<UserOperatorLimitResponse>> GetByUserID(int userId)
        {
            var list = await _repository.GetByUserID(userId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<UserOperatorLimitResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateUserOperatorLimitRequest request)
        {
            var entity = new UserOperatorLimit
            {
                UserID = request.UserID,
                OperatorID = request.OperatorID,
                ServiceID = request.ServiceID,
                LimitAmount = request.LimitAmount,
                IsActive = request.IsActive,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateUserOperatorLimitRequest request)
        {
            var entity = await _repository.GetByID(request.UserOperatorLimitID);
            if (entity == null) return UserOperatorLimitMessages.NotFound;
            entity.UserID = request.UserID;
            entity.OperatorID = request.OperatorID;
            entity.ServiceID = request.ServiceID;
            entity.LimitAmount = request.LimitAmount;
            entity.IsActive = request.IsActive;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : UserOperatorLimitMessages.NotFound;
        }

        private static UserOperatorLimitResponse MapToResponse(UserOperatorLimit e) => new()
        {
            UserOperatorLimitID = e.UserOperatorLimitID,
            UserID = e.UserID,
            OperatorID = e.OperatorID,
            ServiceID = e.ServiceID,
            LimitAmount = e.LimitAmount,
            IsActive = e.IsActive,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };
    }
}

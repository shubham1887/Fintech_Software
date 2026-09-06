using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserCreationSetting;
using FintechApp.Responses.UserCreationSetting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class UserCreationSettingService : IUserCreationSettingService
    {
        private readonly IUserCreationSettingRepository _repository;

        public UserCreationSettingService(IUserCreationSettingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserCreationSettingResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<UserCreationSettingResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<List<UserCreationSettingResponse>> GetByUserID(int userID)
        {
            var list = await _repository.GetByUserID(userID);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<string> Create(CreateUserCreationSettingRequest request)
        {
            var entity = new UserCreationSetting
            {
                UserID = request.UserID,
                Cost = request.Cost,
                MinimumCount = request.MinimumCount,
                DateTime = DateTime.UtcNow
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateUserCreationSettingRequest request)
        {
            var entity = await _repository.GetByID(request.UserCreationSettingID);
            if (entity == null) return Messages.UserCreationSettingMessages.NotFound;
            entity.UserID = request.UserID;
            entity.Cost = request.Cost;
            entity.MinimumCount = request.MinimumCount;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.UserCreationSettingMessages.NotFound;
        }

        private static UserCreationSettingResponse MapToResponse(UserCreationSetting e) => new()
        {
            UserCreationSettingID = e.UserCreationSettingID,
            UserID = e.UserID,
            Cost = e.Cost,
            MinimumCount = e.MinimumCount,
            DateTime = e.DateTime
        };
    }
}

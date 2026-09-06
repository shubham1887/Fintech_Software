using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.UserServiceMapping;
using FintechApp.Responses.UserServiceMapping;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class UserServiceMappingService : IUserServiceMappingService
    {
        private readonly IUserServiceMappingRepository _repository;

        public UserServiceMappingService(IUserServiceMappingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserServiceMappingResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<UserServiceMappingResponse>> GetByUserID(int userId)
        {
            var list = await _repository.GetByUserID(userId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<UserServiceMappingResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateUserServiceMappingRequest request)
        {
            var entity = new UserServiceMapping
            {
                UserID = request.UserID,
                ServiceID = request.ServiceID,
                IsActive = request.IsActive
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateUserServiceMappingRequest request)
        {
            var entity = await _repository.GetByID(request.UserServiceMappingID);
            if (entity == null) return UserServiceMappingMessages.NotFound;
            entity.UserID = request.UserID;
            entity.ServiceID = request.ServiceID;
            entity.IsActive = request.IsActive;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : UserServiceMappingMessages.NotFound;
        }

        private static UserServiceMappingResponse MapToResponse(UserServiceMapping e) => new()
        {
            UserServiceMappingID = e.UserServiceMappingID,
            UserID = e.UserID,
            ServiceID = e.ServiceID,
            IsActive = e.IsActive
        };
    }
}

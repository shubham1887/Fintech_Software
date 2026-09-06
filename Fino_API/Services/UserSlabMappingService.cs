using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserSlabMapping;
using FintechApp.Responses.UserSlabMapping;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class UserSlabMappingService : IUserSlabMappingService
    {
        private readonly IUserSlabMappingRepository _repository;

        public UserSlabMappingService(IUserSlabMappingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserSlabMappingResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<List<UserSlabMappingResponse>> GetByUserID(int userId)
        {
            var list = await _repository.GetByUserID(userId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<UserSlabMappingResponse?> GetByID(int userSlabMappingId)
        {
            var entity = await _repository.GetByID(userSlabMappingId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateUserSlabMappingRequest request)
        {
            var entity = new UserSlabMapping
            {
                UserID = request.UserID,
                SlabMasterID = request.SlabMasterID,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };
            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateUserSlabMappingRequest request)
        {
            var entity = await _repository.GetByID(request.UserSlabMappingID);
            if (entity == null) return Messages.UserSlabMappingMessages.NotFound;

            entity.UserID = request.UserID;
            entity.SlabMasterID = request.SlabMasterID;
            entity.UpdatedateTime = DateTime.UtcNow;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int userSlabMappingId)
        {
            var entity = await _repository.GetByID(userSlabMappingId);
            if (entity == null) return Messages.UserSlabMappingMessages.NotFound;
            int result = await _repository.Delete(userSlabMappingId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static UserSlabMappingResponse MapToResponse(UserSlabMapping e) => new()
        {
            UserSlabMappingID = e.UserSlabMappingID,
            UserID = e.UserID,
            SlabMasterID = e.SlabMasterID,
            SlabName = e.SlabMaster?.SlabName ?? string.Empty,
            DateTime = e.DateTime
        };
    }
}

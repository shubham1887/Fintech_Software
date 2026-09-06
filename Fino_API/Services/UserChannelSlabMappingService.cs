using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserChannelSlabMapping;
using FintechApp.Responses.UserChannelSlabMapping;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class UserChannelSlabMappingService : IUserChannelSlabMappingService
    {
        private readonly IUserChannelSlabMappingRepository _repository;

        public UserChannelSlabMappingService(IUserChannelSlabMappingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserChannelSlabMappingResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<List<UserChannelSlabMappingResponse>> GetByUserID(int userId)
        {
            var list = await _repository.GetByUserID(userId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<UserChannelSlabMappingResponse?> GetByID(int userChannelSlabMappingId)
        {
            var entity = await _repository.GetByID(userChannelSlabMappingId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateUserChannelSlabMappingRequest request)
        {
            var entity = new UserChannelSlabMapping
            {
                UserID = request.UserID,
                ChannelSlabMasterID = request.ChannelSlabMasterID,
                UplineUserID = request.UplineUserID,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };
            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateUserChannelSlabMappingRequest request)
        {
            var entity = await _repository.GetByID(request.UserChannelSlabMappingID);
            if (entity == null) return Messages.UserChannelSlabMappingMessages.NotFound;

            entity.UserID = request.UserID;
            entity.ChannelSlabMasterID = request.ChannelSlabMasterID;
            entity.UplineUserID = request.UplineUserID;
            entity.UpdatedateTime = DateTime.UtcNow;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int userChannelSlabMappingId)
        {
            var entity = await _repository.GetByID(userChannelSlabMappingId);
            if (entity == null) return Messages.UserChannelSlabMappingMessages.NotFound;
            int result = await _repository.Delete(userChannelSlabMappingId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static UserChannelSlabMappingResponse MapToResponse(UserChannelSlabMapping e) => new()
        {
            UserChannelSlabMappingID = e.UserChannelSlabMappingID,
            UserID = e.UserID,
            ChannelSlabMasterID = e.ChannelSlabMasterID,
            ChannelSlabName = e.ChannelSlabMaster?.SlabName ?? string.Empty,
            UplineUserID = e.UplineUserID,
            DateTime = e.DateTime
        };
    }
}

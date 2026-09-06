using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ChannelSlabMaster;
using FintechApp.Responses.ChannelSlabMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ChannelSlabMasterService : IChannelSlabMasterService
    {
        private readonly IChannelSlabMasterRepository _repository;

        public ChannelSlabMasterService(IChannelSlabMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ChannelSlabMasterResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<List<ChannelSlabMasterResponse>> GetByUplineUser(int uplineUserId)
        {
            var list = await _repository.GetByUplineUser(uplineUserId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<ChannelSlabMasterResponse?> GetByID(int channelSlabMasterId)
        {
            var entity = await _repository.GetByID(channelSlabMasterId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<List<ChannelSlabMasterDropdownResponse>> GetDropdown(int uplineUserId)
        {
            var list = uplineUserId > 0
                ? await _repository.GetByUplineUser(uplineUserId)
                : await _repository.GetAll();

            return list.Where(x => x.IsActive)
                       .Select(x => new ChannelSlabMasterDropdownResponse
                       {
                           ChannelSlabMasterID = x.ChannelSlabMasterID,
                           SlabName = x.SlabName
                       }).ToList();
        }

        public async Task<string> Create(CreateChannelSlabMasterRequest request)
        {
            var entity = new ChannelSlabMaster
            {
                SlabName = request.SlabName,
                IsActive = request.IsActive,
                UplineUserID = request.UplineUserID,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateChannelSlabMasterRequest request)
        {
            var entity = await _repository.GetByID(request.ChannelSlabMasterID);
            if (entity == null) return Messages.ChannelSlabMasterMessages.NotFound;

            entity.SlabName = request.SlabName;
            entity.IsActive = request.IsActive;
            entity.UplineUserID = request.UplineUserID;
            entity.UpdatedateTime = DateTime.UtcNow;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int channelSlabMasterId)
        {
            var entity = await _repository.GetByID(channelSlabMasterId);
            if (entity == null) return Messages.ChannelSlabMasterMessages.NotFound;

            int result = await _repository.Delete(channelSlabMasterId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static ChannelSlabMasterResponse MapToResponse(ChannelSlabMaster e) => new()
        {
            ChannelSlabMasterID = e.ChannelSlabMasterID,
            SlabName = e.SlabName,
            IsActive = e.IsActive,
            UplineUserID = e.UplineUserID,
            DateTime = e.DateTime
        };
    }
}

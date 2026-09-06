using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.MediaMaster;
using FintechApp.Responses.MediaMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class MediaMasterService : IMediaMasterService
    {
        private readonly IMediaMasterRepository _repository;
        public MediaMasterService(IMediaMasterRepository repository) { _repository = repository; }

        private static MediaMasterResponse MapToResponse(MediaMaster e) => new()
        {
            MediaMasterID = e.MediaMasterID,
            UserID = e.UserID,
            MediaType = e.MediaType,
            MediaPath = e.MediaPath,
        };

        public async Task<List<MediaMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<MediaMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<MediaMasterResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateMediaMasterRequest request)
        {
            var entity = new MediaMaster
            {
                UserID = request.UserID,
                MediaType = request.MediaType,
                MediaPath = request.MediaPath,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateMediaMasterRequest request)
        {
            var entity = await _repository.GetByID(request.MediaMasterID);
            if (entity == null) return Messages.MediaMasterMessages.NotFound;
            entity.UserID = request.UserID;
            entity.MediaType = request.MediaType;
            entity.MediaPath = request.MediaPath;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.MediaMasterMessages.NotFound;
        }
    }
}

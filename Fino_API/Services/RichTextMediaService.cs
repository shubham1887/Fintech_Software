using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.RichTextMedia;
using FintechApp.Responses.RichTextMedia;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class RichTextMediaService : IRichTextMediaService
    {
        private readonly IRichTextMediaRepository _repository;
        public RichTextMediaService(IRichTextMediaRepository repository) { _repository = repository; }

        private static RichTextMediaResponse MapToResponse(RichTextMedia e) => new()
        {
            ID = e.ID,
            Key = e.Key,
            Content = e.Content,
        };

        public async Task<List<RichTextMediaResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<RichTextMediaResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateRichTextMediaRequest request)
        {
            var entity = new RichTextMedia
            {
                Key = request.Key,
                Content = request.Content,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateRichTextMediaRequest request)
        {
            var entity = await _repository.GetByID(request.ID);
            if (entity == null) return Messages.RichTextMediaMessages.NotFound;
            entity.Key = request.Key;
            entity.Content = request.Content;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.RichTextMediaMessages.NotFound;
        }
    }
}

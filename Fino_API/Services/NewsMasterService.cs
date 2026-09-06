using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.News;
using FintechApp.Responses.News;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class NewsMasterService : INewsMasterService
    {
        private readonly INewsMasterRepository _repository;

        public NewsMasterService(INewsMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<NewsResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<NewsResponse?> GetByID(int newsId)
        {
            var entity = await _repository.GetByID(newsId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateNewsRequest request)
        {
            var entity = new NewsMaster
            {
                UserID = request.UserID,
                Title = request.Title,
                Description = request.Description,
                IsView = request.IsView,
                ImagePath = request.ImagePath,
                HyperLink = request.HyperLink,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateNewsRequest request)
        {
            var entity = await _repository.GetByID(request.NewsID);
            if (entity == null) return Messages.NewsMasterMessages.NotFound;

            entity.UserID = request.UserID;
            entity.Title = request.Title;
            entity.Description = request.Description;
            entity.IsView = request.IsView;
            entity.ImagePath = request.ImagePath;
            entity.HyperLink = request.HyperLink;
            entity.UpdatedateTime = DateTime.UtcNow;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int newsId)
        {
            var entity = await _repository.GetByID(newsId);
            if (entity == null) return Messages.NewsMasterMessages.NotFound;

            int result = await _repository.Delete(newsId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static NewsResponse MapToResponse(NewsMaster e) => new()
        {
            NewsID = e.NewsID,
            UserID = e.UserID,
            Title = e.Title,
            Description = e.Description,
            IsView = e.IsView,
            ImagePath = e.ImagePath,
            HyperLink = e.HyperLink,
            DateTime = e.DateTime
        };
    }
}

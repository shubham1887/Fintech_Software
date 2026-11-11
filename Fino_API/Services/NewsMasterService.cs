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

        /// <summary>
        /// What the network should actually see: published, and not past its expiry.
        ///
        /// The admin's own list (GetAll) keeps everything, drafts and expired items included, so
        /// a lapsed offer can be re-dated or deleted rather than vanishing from the screen that
        /// manages it.
        /// </summary>
        public async Task<List<NewsResponse>> GetPublished()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).Where(n => n.IsView && !n.IsExpired).ToList();
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
                ExpiresOn = Normalise(request.ExpiresOn),
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

            // The author stays whoever published it; an edit does not reassign authorship.
            entity.Title = request.Title;
            entity.Description = request.Description;
            entity.IsView = request.IsView;
            entity.ImagePath = request.ImagePath;
            entity.HyperLink = request.HyperLink;
            entity.ExpiresOn = Normalise(request.ExpiresOn);
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

        /// <summary>
        /// A date with no time of day means "to the end of that day", which is what an admin who
        /// typed 31 March expects — storing it as 00:00 would retire the notice a day early.
        /// </summary>
        private static DateTime? Normalise(DateTime? expiresOn)
        {
            if (expiresOn is not DateTime on) return null;
            return on.TimeOfDay == TimeSpan.Zero ? on.Date.AddDays(1).AddSeconds(-1) : on;
        }

        /// <summary>Expiry is a wall-clock moment the admin typed, so it is read in local time.</summary>
        private static bool HasLapsed(DateTime? expiresOn) => expiresOn is DateTime on && on <= DateTime.Now;

        private static NewsResponse MapToResponse(NewsMaster e) => new()
        {
            NewsID = e.NewsID,
            UserID = e.UserID,
            PublishedBy = e.UserMaster?.PersonName ?? string.Empty,
            Title = e.Title,
            Description = e.Description,
            IsView = e.IsView,
            ImagePath = e.ImagePath,
            HyperLink = e.HyperLink,
            DateTime = e.DateTime,
            ExpiresOn = e.ExpiresOn,
            IsExpired = HasLapsed(e.ExpiresOn)
        };
    }
}

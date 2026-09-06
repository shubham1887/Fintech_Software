using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.WebsitePage;
using FintechApp.Responses.WebsitePage;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class WebsitePageService : IWebsitePageService
    {
        private readonly IWebsitePageRepository _repository;

        public WebsitePageService(IWebsitePageRepository repository)
        {
            _repository = repository;
        }

        private static WebsitePageResponse MapToResponse(WebsitePageMaster e) => new()
        {
            WebsitePageMasterID = e.WebsitePageMasterID,
            Slug = e.Slug,
            Title = e.Title,
            MetaDescription = e.MetaDescription,
            SectionsJson = e.SectionsJson,
            MenuOrder = e.MenuOrder,
            ShowInMenu = e.ShowInMenu,
            IsPublished = e.IsPublished,
        };

        public async Task<List<WebsitePageResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<WebsitePageResponse>> GetPublished()
        {
            var list = await _repository.GetAll();
            return list.Where(x => x.IsPublished).OrderBy(x => x.MenuOrder).Select(MapToResponse).ToList();
        }

        public async Task<WebsitePageResponse?> GetByID(int id)
        {
            var e = await _repository.GetByID(id);
            return e == null ? null : MapToResponse(e);
        }

        public async Task<string> Create(CreateWebsitePageRequest request)
        {
            var all = await _repository.GetAll();
            string slug = request.Slug.Trim().ToLowerInvariant();
            if (all.Any(x => x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase)))
                return Messages.WebsitePageMessages.SlugAlreadyExists;

            var entity = new WebsitePageMaster
            {
                Slug = slug,
                Title = request.Title.Trim(),
                MetaDescription = request.MetaDescription ?? string.Empty,
                SectionsJson = request.SectionsJson ?? string.Empty,
                MenuOrder = request.MenuOrder,
                ShowInMenu = request.ShowInMenu,
                IsPublished = request.IsPublished,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateWebsitePageRequest request)
        {
            var entity = await _repository.GetByID(request.WebsitePageMasterID);
            if (entity == null) return Messages.WebsitePageMessages.NotFound;

            var all = await _repository.GetAll();
            string slug = request.Slug.Trim().ToLowerInvariant();
            if (all.Any(x => x.WebsitePageMasterID != request.WebsitePageMasterID &&
                             x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase)))
                return Messages.WebsitePageMessages.SlugAlreadyExists;

            entity.Slug = slug;
            entity.Title = request.Title.Trim();
            entity.MetaDescription = request.MetaDescription ?? string.Empty;
            entity.SectionsJson = request.SectionsJson ?? string.Empty;
            entity.MenuOrder = request.MenuOrder;
            entity.ShowInMenu = request.ShowInMenu;
            entity.IsPublished = request.IsPublished;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.WebsitePageMessages.NotFound;
        }
    }
}

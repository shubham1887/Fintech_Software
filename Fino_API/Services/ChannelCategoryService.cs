using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ChannelCategory;
using FintechApp.Responses.ChannelCategory;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class ChannelCategoryService : IChannelCategoryService
    {
        private readonly IChannelCategoryRepository _repository;
        public ChannelCategoryService(IChannelCategoryRepository repository) { _repository = repository; }
        private static ChannelCategoryResponse MapToResponse(ChannelCategory e) => new() { ChannelCategoryID = e.ChannelCategoryID, Category = e.Category, };
        public async Task<List<ChannelCategoryResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<ChannelCategoryResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateChannelCategoryRequest request) { var entity = new ChannelCategory { Category = request.Category, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateChannelCategoryRequest request) { var entity = await _repository.GetByID(request.ChannelCategoryID); if (entity == null) return Messages.ChannelCategoryMessages.NotFound; entity.Category = request.Category; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.ChannelCategoryMessages.NotFound; }
    }
}

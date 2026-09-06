using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.CategoryChannelMapping;
using FintechApp.Responses.CategoryChannelMapping;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class CategoryChannelMappingService : ICategoryChannelMappingService
    {
        private readonly ICategoryChannelMappingRepository _repository;
        public CategoryChannelMappingService(ICategoryChannelMappingRepository repository) { _repository = repository; }
        private static CategoryChannelMappingResponse MapToResponse(CategoryChannelMapping e) => new() { CategoryChannelMappingID = e.CategoryChannelMappingID, ChannelCategoryID = e.ChannelCategoryID, ChannelName = e.ChannelName, ChannelImagePath = e.ChannelImagePath, IsDelete = e.IsDelete, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<CategoryChannelMappingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<CategoryChannelMappingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateCategoryChannelMappingRequest request) { var entity = new CategoryChannelMapping { ChannelCategoryID = request.ChannelCategoryID, ChannelName = request.ChannelName, ChannelImagePath = request.ChannelImagePath, IsDelete = request.IsDelete, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateCategoryChannelMappingRequest request) { var entity = await _repository.GetByID(request.CategoryChannelMappingID); if (entity == null) return Messages.CategoryChannelMappingMessages.NotFound; entity.ChannelCategoryID = request.ChannelCategoryID; entity.ChannelName = request.ChannelName; entity.ChannelImagePath = request.ChannelImagePath; entity.IsDelete = request.IsDelete; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.CategoryChannelMappingMessages.NotFound; }
    }
}

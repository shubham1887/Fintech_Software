using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.SubCategory;
using FintechApp.Responses.SubCategory;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class SubCategoryService : ISubCategoryService
    {
        private readonly ISubCategoryRepository _repository;
        public SubCategoryService(ISubCategoryRepository repository) { _repository = repository; }
        private static SubCategoryResponse MapToResponse(SubCategory e) => new() { SubCategoryID = e.SubCategoryID, CategoryID = e.CategoryID, SubCategoryName = e.SubCategoryName, ShortDescription = e.ShortDescription, LongDescription = e.LongDescription, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<SubCategoryResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<SubCategoryResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateSubCategoryRequest request) { var entity = new SubCategory { CategoryID = request.CategoryID, SubCategoryName = request.SubCategoryName, ShortDescription = request.ShortDescription, LongDescription = request.LongDescription, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateSubCategoryRequest request) { var entity = await _repository.GetByID(request.SubCategoryID); if (entity == null) return Messages.SubCategoryMessages.NotFound; entity.CategoryID = request.CategoryID; entity.SubCategoryName = request.SubCategoryName; entity.ShortDescription = request.ShortDescription; entity.LongDescription = request.LongDescription; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.SubCategoryMessages.NotFound; }
    }
}

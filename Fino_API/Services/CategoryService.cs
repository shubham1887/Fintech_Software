using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Category;
using FintechApp.Responses.Category;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        public CategoryService(ICategoryRepository repository) { _repository = repository; }
        private static CategoryResponse MapToResponse(Category e) => new() { CategoryID = e.CategoryID, CategoryName = e.CategoryName, };
        public async Task<List<CategoryResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<CategoryResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateCategoryRequest request) { var entity = new Category { CategoryName = request.CategoryName, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateCategoryRequest request) { var entity = await _repository.GetByID(request.CategoryID); if (entity == null) return Messages.CategoryMessages.NotFound; entity.CategoryName = request.CategoryName; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.CategoryMessages.NotFound; }
    }
}

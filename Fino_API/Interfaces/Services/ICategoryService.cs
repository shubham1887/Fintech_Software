using FintechApp.Requests.Category;
using FintechApp.Responses.Category;
namespace FintechApp.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryResponse>> GetAll();
        Task<CategoryResponse?> GetByID(int id);

        Task<string> Create(CreateCategoryRequest request);
        Task<string> Update(UpdateCategoryRequest request);
        Task<string> Delete(int id);
    }
}

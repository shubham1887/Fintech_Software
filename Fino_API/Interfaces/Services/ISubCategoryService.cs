using FintechApp.Requests.SubCategory;
using FintechApp.Responses.SubCategory;
namespace FintechApp.Interfaces.Services
{
    public interface ISubCategoryService
    {
        Task<List<SubCategoryResponse>> GetAll();
        Task<SubCategoryResponse?> GetByID(int id);

        Task<string> Create(CreateSubCategoryRequest request);
        Task<string> Update(UpdateSubCategoryRequest request);
        Task<string> Delete(int id);
    }
}

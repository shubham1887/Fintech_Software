using FintechApp.Requests.CategoryChannelMapping;
using FintechApp.Responses.CategoryChannelMapping;
namespace FintechApp.Interfaces.Services
{
    public interface ICategoryChannelMappingService
    {
        Task<List<CategoryChannelMappingResponse>> GetAll();
        Task<CategoryChannelMappingResponse?> GetByID(int id);

        Task<string> Create(CreateCategoryChannelMappingRequest request);
        Task<string> Update(UpdateCategoryChannelMappingRequest request);
        Task<string> Delete(int id);
    }
}

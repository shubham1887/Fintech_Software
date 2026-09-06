using FintechApp.Requests.ChannelCategory;
using FintechApp.Responses.ChannelCategory;
namespace FintechApp.Interfaces.Services
{
    public interface IChannelCategoryService
    {
        Task<List<ChannelCategoryResponse>> GetAll();
        Task<ChannelCategoryResponse?> GetByID(int id);

        Task<string> Create(CreateChannelCategoryRequest request);
        Task<string> Update(UpdateChannelCategoryRequest request);
        Task<string> Delete(int id);
    }
}

using FintechApp.Requests.RichTextMedia;
using FintechApp.Responses.RichTextMedia;
namespace FintechApp.Interfaces.Services
{
    public interface IRichTextMediaService
    {
        Task<List<RichTextMediaResponse>> GetAll();
        Task<RichTextMediaResponse?> GetByID(int id);
        Task<string> Create(CreateRichTextMediaRequest request);
        Task<string> Update(UpdateRichTextMediaRequest request);
        Task<string> Delete(int id);
    }
}

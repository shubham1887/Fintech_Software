using FintechApp.Requests.ExternalMedia;
using FintechApp.Responses.ExternalMedia;
namespace FintechApp.Interfaces.Services
{
    public interface IExternalMediaService
    {
        Task<List<ExternalMediaResponse>> GetAll();
        Task<ExternalMediaResponse?> GetByID(int id);
        Task<string> Create(CreateExternalMediaRequest request);
        Task<string> Update(UpdateExternalMediaRequest request);
        Task<string> Delete(int id);
    }
}

using FintechApp.Requests.MediaMaster;
using FintechApp.Responses.MediaMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IMediaMasterService
    {
        Task<List<MediaMasterResponse>> GetAll();
        Task<MediaMasterResponse?> GetByID(int id);
        Task<List<MediaMasterResponse>> GetByUserID(int userID);
        Task<string> Create(CreateMediaMasterRequest request);
        Task<string> Update(UpdateMediaMasterRequest request);
        Task<string> Delete(int id);
    }
}
